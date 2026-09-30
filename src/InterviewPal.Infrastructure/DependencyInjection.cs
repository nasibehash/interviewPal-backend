using InterviewPal.Application.Abstractions;
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
        services.AddDbContext<AppDbContext>((sp, o) => o.UseSqlite(
            sp.GetRequiredService<IConfiguration>().GetConnectionString("Default") ?? "Data Source=interviewpal.db"));
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<ContentSeeder>();
        services.AddSingleton<LessonCatalog>();
        services.AddSingleton<ILessonCatalog>(sp => sp.GetRequiredService<LessonCatalog>());
        return services;
    }

    /// <summary>Creates the database if needed and loads the question bank from <paramref name="contentDirectory"/>.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider provider, string contentDirectory)
    {
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<ContentSeeder>().SeedAsync(contentDirectory);

        var technologies = await db.Technologies.Select(t => t.Slug).ToListAsync();
        provider.GetRequiredService<LessonCatalog>().Load(Path.Combine(contentDirectory, "lessons"), technologies);
    }
}
