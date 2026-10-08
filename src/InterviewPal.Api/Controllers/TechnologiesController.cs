using Microsoft.AspNetCore.Authorization;
using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPal.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/technologies")]
public class TechnologiesController(QuestionService questions) : ControllerBase
{
    /// <summary>All technologies with question counts per level.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<TechnologyDto>> GetAll(CancellationToken ct) =>
        await questions.GetTechnologiesAsync(ct);
}
