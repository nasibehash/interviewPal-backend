using InterviewPal.Api.Auth;
using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace InterviewPal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService auth, ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Creates an account and logs in. The refresh token is set as an httpOnly cookie.</summary>
    [HttpPost("register")]
    [EnableRateLimiting(AuthExtensions.RateLimitPolicy)]
    public async Task<AuthResponse> Register(RegisterRequest request, CancellationToken ct) =>
        WithCookie(await auth.RegisterAsync(request, ct));

    [HttpPost("login")]
    [EnableRateLimiting(AuthExtensions.RateLimitPolicy)]
    public async Task<AuthResponse> Login(LoginRequest request, CancellationToken ct) =>
        WithCookie(await auth.LoginAsync(request, ct));

    /// <summary>New access token from the refresh cookie (and a rotated cookie).</summary>
    [HttpPost("refresh")]
    [EnableRateLimiting(AuthExtensions.RateLimitPolicy)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        try
        {
            return WithCookie(await auth.RefreshAsync(RefreshCookie.Read(Request), ct));
        }
        catch (UnauthorizedException e)
        {
            // answered here, not through the exception handler: it would reset the response and lose the cookie deletion
            RefreshCookie.Clear(Response);
            return Problem(detail: e.Message, statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await auth.LogoutAsync(RefreshCookie.Read(Request), ct);
        RefreshCookie.Clear(Response);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<UserDto> Me(CancellationToken ct) => await auth.GetAsync(currentUser.Id!.Value, ct);

    /// <summary>Changes the password and ends every session (the learner logs in again).</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(currentUser.Id!.Value, request, ct);
        RefreshCookie.Clear(Response);
        return NoContent();
    }

    /// <summary>Deletes the account and its progress. The password confirms it.</summary>
    [HttpPost("delete-account")]
    [Authorize]
    public async Task<IActionResult> Delete(LoginRequestPassword request, CancellationToken ct)
    {
        await auth.DeleteAsync(currentUser.Id!.Value, request.Password, ct);
        RefreshCookie.Clear(Response);
        return NoContent();
    }

    private AuthResponse WithCookie(AuthResult result)
    {
        RefreshCookie.Set(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return result.Response;
    }
}

public record LoginRequestPassword
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(128)]
    public string Password { get; init; } = "";
}
