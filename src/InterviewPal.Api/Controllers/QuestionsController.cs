using Microsoft.AspNetCore.Authorization;
using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/questions")]
public class QuestionsController(QuestionService questions, ReportService reports) : ControllerBase
{
    /// <summary>Browse questions (answers are not included).</summary>
    [HttpGet]
    public async Task<PagedResult<QuestionDto>> Search(
        [FromQuery] string? technology,
        [FromQuery] string? level,
        [FromQuery] string? tag,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default) =>
        await questions.SearchAsync(technology, level, tag, search, page, pageSize, ct);

    /// <summary>A single question together with its answer and explanation.</summary>
    [HttpGet("{id}")]
    public async Task<QuestionDetailDto> Get(string id, CancellationToken ct) =>
        await questions.GetAsync(id, ct);

    /// <summary>Report a wrong, outdated or unclear question.</summary>
    [HttpPost("{id}/reports")]
    public async Task<IActionResult> Report(string id, ReportQuestionRequest request, CancellationToken ct)
    {
        await reports.ReportAsync(id, request, ct);
        return NoContent();
    }
}
