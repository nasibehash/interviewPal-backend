using System.ComponentModel.DataAnnotations;

namespace InterviewPal.Application.Contracts;

public record RegisterRequest
{
    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; init; } = "";

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; init; } = "";

    [Required, MinLength(2), MaxLength(40)]
    public string DisplayName { get; init; } = "";
}

public record LoginRequest
{
    [Required, MaxLength(254)]
    public string Email { get; init; } = "";

    [Required, MaxLength(128)]
    public string Password { get; init; } = "";
}

public record UserDto(Guid Id, string Email, string DisplayName, DateTime CreatedAt);

/// <summary>The short-lived access token and who it belongs to. The refresh token travels in an httpOnly cookie.</summary>
public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);

/// <summary>What the controller needs after login/register/refresh: the response body plus the new refresh token.</summary>
public record AuthResult(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiresAt);

public record ChangePasswordRequest
{
    [Required, MaxLength(128)]
    public string CurrentPassword { get; init; } = "";

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; init; } = "";
}
