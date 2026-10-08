using InterviewPal.Api.Auth;
using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPal.Api.Controllers;

/// <summary>The logged-in learner's own progress.</summary>
[ApiController]
[Authorize]
[Route("api/me/progress")]
public class MeController(ProgressService progress, ICurrentUser currentUser) : ControllerBase
{
    private Guid UserId => currentUser.Id!.Value;

    /// <summary>Practice history, per-question stats and lesson exercise answers.</summary>
    [HttpGet]
    public async Task<ProgressDto> Get(CancellationToken ct) => await progress.GetAsync(UserId, ct);

    /// <summary>Adds the progress collected in the browser before the learner had an account.</summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(ImportProgressRequest request, CancellationToken ct)
    {
        await progress.ImportAsync(UserId, request, ct);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await progress.ClearAsync(UserId, ct);
        return NoContent();
    }
}
