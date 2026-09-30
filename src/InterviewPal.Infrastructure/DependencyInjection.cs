using InterviewPal.Application.Abstractions;
using InterviewPal.Infrastructure.Persistence;
using InterviewPal.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ContentSeeder>();
        return services;
    }

    /// <summary>Creates the database if needed and loads the question bank from <paramref name="contentDirectory"/>.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider provider, string contentDirectory)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<ContentSeeder>().SeedAsync(contentDirectory);
    }
}
