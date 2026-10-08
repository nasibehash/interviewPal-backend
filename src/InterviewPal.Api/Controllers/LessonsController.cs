using Microsoft.AspNetCore.Authorization;
using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/lessons")]
public class LessonsController(LessonService lessons) : ControllerBase
{
    /// <summary>Lessons on algorithms and design patterns. Filters: kind, level, category, technology.</summary>
    [HttpGet]
    public IReadOnlyList<LessonSummaryDto> List(
        [FromQuery] string? kind, [FromQuery] string? level, [FromQuery] string? category, [FromQuery] string? technology) =>
        lessons.List(kind, level, category, technology);

    /// <summary>One lesson; pass <c>technology</c> to get the implementation written for it.</summary>
    [HttpGet("{id}")]
    public LessonDetailDto Get(string id, [FromQuery] string? technology) => lessons.Get(id, technology);

    /// <summary>Grade one exercise of a lesson.</summary>
    [HttpPost("{id}/exercises/{exerciseId}/check")]
    public async Task<CheckExerciseResult> Check(string id, string exerciseId, CheckExerciseRequest request, CancellationToken ct) =>
        await lessons.CheckAsync(id, exerciseId, request, ct);
}
