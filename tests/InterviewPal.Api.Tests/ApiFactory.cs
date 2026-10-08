using InterviewPal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace InterviewPal.Api.Tests;

/// <summary>
/// The real application on its own database. SQLite by default; set TEST_POSTGRES_ADMIN (a connection string that may
/// create databases, e.g. "Host=localhost;Username=postgres;Password=postgres;Database=postgres") to run the same tests
/// against PostgreSQL with migrations.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"interviewpal-test-{Guid.NewGuid():N}.db");
    private readonly string? _postgresAdmin = Environment.GetEnvironmentVariable("TEST_POSTGRES_ADMIN");
    private readonly string _postgresDatabase = $"interviewpal_test_{Guid.NewGuid():N}";

    /// <summary>Extra settings a test class wants (for example a tiny rate limit).</summary>
    protected virtual Dictionary<string, string?> Settings => [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Content:Path"] = Path.Combine(AppContext.BaseDirectory, "TestContent"),
                ["Auth:JwtKey"] = "test-signing-key-test-signing-key-test-signing-key",
                ["Auth:RequestsPerMinute"] = "100000" // many logins from one address in one test run
            };

            if (_postgresAdmin is null)
            {
                settings["ConnectionStrings:Default"] = $"Data Source={_dbPath}";
            }
            else
            {
                using (var connection = new NpgsqlConnection(_postgresAdmin))
                {
                    connection.Open();
                    using var create = new NpgsqlCommand($"CREATE DATABASE {_postgresDatabase}", connection);
                    create.ExecuteNonQuery();
                }

                settings["Database:Provider"] = "Postgres";
                settings["ConnectionStrings:Default"] =
                    new NpgsqlConnectionStringBuilder(_postgresAdmin) { Database = _postgresDatabase }.ConnectionString;
            }

            foreach (var (key, value) in Settings) settings[key] = value;
            config.AddInMemoryCollection(settings);
        });
    }

    public async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var f in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" })
            if (File.Exists(f)) File.Delete(f);

        if (_postgresAdmin is not null && disposing)
        {
            NpgsqlConnection.ClearAllPools();
            using var connection = new NpgsqlConnection(_postgresAdmin);
            connection.Open();
            using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS {_postgresDatabase} WITH (FORCE)", connection);
            drop.ExecuteNonQuery();
        }
    }
}
