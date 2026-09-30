using InterviewPal.Domain;

namespace InterviewPal.Application.Abstractions;

public interface IReportRepository
{
    Task<bool> QuestionExistsAsync(string questionId, CancellationToken ct);
    Task AddAsync(QuestionReport report, CancellationToken ct);
}
