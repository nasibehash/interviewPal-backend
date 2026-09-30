using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPal.Api.Controllers;

[ApiController]
[Route("api/practice")]
public class PracticeController(PracticeService practice) : ControllerBase
{
    /// <summary>Builds a practice session: a balanced random selection matching the filters.</summary>
    [HttpPost("sessions")]
    public async Task<PracticeSessionDto> Start(StartPracticeRequest request, CancellationToken ct) =>
        await practice.StartAsync(request, ct);

    /// <summary>Grades one answer and returns the explanation (learning and flashcard modes).</summary>
    [HttpPost("questions/{questionId}/check")]
    public async Task<CheckAnswerResult> Check(string questionId, CheckAnswerRequest request, CancellationToken ct) =>
        await practice.CheckAsync(questionId, request, ct);

    /// <summary>Grades a whole session at once (interview mode) and reports weak topics.</summary>
    [HttpPost("evaluate")]
    public async Task<EvaluationResult> Evaluate(EvaluatePracticeRequest request, CancellationToken ct) =>
        await practice.EvaluateAsync(request, ct);
}
