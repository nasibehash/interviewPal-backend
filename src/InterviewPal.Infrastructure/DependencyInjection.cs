using InterviewPal.Application.Abstractions;
using InterviewPal.Application;
using InterviewPal.Infrastructure.Auth;
using InterviewPal.Infrastructure.Lessons;
using InterviewPal.Infrastructure.Persistence;
using InterviewPal.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPal.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers persistence. The connection string is resolved lazily so test/host overrides apply.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((sp, o) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connection = configuration.GetConnectionString("Default") ?? "Data Source=interviewpal.db";
            if (UsesPostgres(configuration)) o.UseNpgsql(PostgresConnection.Normalize(connection));
            else o.UseSqlite(connection);
        });
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<ContentSeeder>();
        services.AddSingleton<LessonCatalog>();
        services.AddSingleton<ILessonCatalog>(sp => sp.GetRequiredService<LessonCatalog>());
        return services;
    }

    /// <summary>"Postgres" in <c>Database:Provider</c> selects PostgreSQL; anything else (the default) is SQLite.</summary>
    public static bool UsesPostgres(IConfiguration configuration) =>
        string.Equals(configuration["Database:Provider"], "Postgres", StringComparison.OrdinalIgnoreCase);

    /// <summary>Creates the database if needed and loads the question bank from <paramref name="contentDirectory"/>.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider provider, string contentDirectory)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // PostgreSQL (production) evolves through migrations; SQLite (development and tests) is created from the model.
        if (UsesPostgres(provider.GetRequiredService<IConfiguration>())) await db.Database.MigrateAsync();
        else await db.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<ContentSeeder>().SeedAsync(contentDirectory);

        var technologies = await db.Technologies.Select(t => t.Slug).ToListAsync();
        provider.GetRequiredService<LessonCatalog>().Load(Path.Combine(contentDirectory, "lessons"), technologies);
    }
}
