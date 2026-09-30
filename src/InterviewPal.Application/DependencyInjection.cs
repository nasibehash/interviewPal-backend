using InterviewPal.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<QuestionService>();
        services.AddScoped<PracticeService>();
        services.AddScoped<ReportService>();
        services.AddScoped<LessonService>();
        return services;
    }
}
