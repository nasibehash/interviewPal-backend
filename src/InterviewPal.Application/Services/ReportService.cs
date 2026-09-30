using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

public class ReportService(IReportRepository reports)
{
    public async Task ReportAsync(string questionId, ReportQuestionRequest request, CancellationToken ct)
    {
        if (request.Reason is null)
            throw new RequestValidationException("A reason is required.");

        if (!await reports.QuestionExistsAsync(questionId, ct))
            throw new NotFoundException($"Question '{questionId}' was not found.");

        await reports.AddAsync(new QuestionReport
        {
            QuestionId = questionId,
            Reason = request.Reason.Value,
            Message = request.Message?.Trim()
        }, ct);
    }
}
