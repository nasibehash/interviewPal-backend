using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Infrastructure.Persistence;

public class ReportRepository(AppDbContext db) : IReportRepository
{
    public Task<bool> QuestionExistsAsync(string questionId, CancellationToken ct) =>
        db.Questions.AnyAsync(q => q.Id == questionId, ct);

    public async Task AddAsync(QuestionReport report, CancellationToken ct)
    {
        db.QuestionReports.Add(report);
        await db.SaveChangesAsync(ct);
    }
}
