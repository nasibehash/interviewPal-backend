using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;
using Microsoft.Extensions.Options;

namespace InterviewPal.Application.Services;

public class AuthService(
    IUserRepository users,
    IProgressRepository progress,
    IPasswordService passwords,
    ITokenService tokens,
    IClock clock,
    IOptions<AuthOptions> options)
{
    private readonly AuthOptions _options = options.Value;

    private const string BadCredentials = "Email or password is not correct.";

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        ValidatePassword(request.Password, email);

        if (await users.FindByEmailAsync(email, ct) is not null)
            throw new ConflictException("An account with this email already exists.");

        var user = new User
        {
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = passwords.Hash(request.Password),
            CreatedAt = clock.UtcNow
        };
        await users.AddAsync(user, ct);
        return await IssueAsync(user, ct);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(NormalizeEmail(request.Email), ct);
        if (user is null)
        {
            passwords.WasteTime(); // same response time whether or not the email exists
            throw new UnauthorizedException(BadCredentials);
        }

        var now = clock.UtcNow;
        if (user.LockoutEnd is { } lockedUntil && lockedUntil > now)
            throw new TooManyRequestsException("Too many failed attempts. Try again later.");

        if (!passwords.Verify(user.PasswordHash, request.Password))
        {
            if (++user.FailedLoginCount >= _options.MaxFailedLogins)
            {
                user.LockoutEnd = now.AddMinutes(_options.LockoutMinutes);
                user.FailedLoginCount = 0;
            }

            await users.SaveChangesAsync(ct);
            throw new UnauthorizedException(BadCredentials);
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        return await IssueAsync(user, ct);
    }

    /// <summary>Exchanges a refresh token for a new access token and a new refresh token (rotation).</summary>
    public async Task<AuthResult> RefreshAsync(string? refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(refreshToken)) throw new UnauthorizedException("No session.");

        var now = clock.UtcNow;
        var stored = await users.FindRefreshTokenAsync(tokens.HashRefreshToken(refreshToken), ct)
                     ?? throw new UnauthorizedException("Session is not valid.");

        if (stored.RevokedAt is not null)
        {
            // A token that was already used is presented again: it may have been stolen. End every session of the user.
            await users.RevokeAllRefreshTokensAsync(stored.UserId, now, ct);
            throw new UnauthorizedException("Session is not valid.");
        }

        if (stored.ExpiresAt <= now) throw new UnauthorizedException("Session has expired.");

        var user = await users.FindByIdAsync(stored.UserId, ct) ?? throw new UnauthorizedException("Session is not valid.");
        stored.RevokedAt = now;
        return await IssueAsync(user, ct);
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;
        var stored = await users.FindRefreshTokenAsync(tokens.HashRefreshToken(refreshToken), ct);
        if (stored is null || stored.RevokedAt is not null) return;
        stored.RevokedAt = clock.UtcNow;
        await users.SaveChangesAsync(ct);
    }

    public async Task<UserDto> GetAsync(Guid userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId, ct) ?? throw new UnauthorizedException("Account no longer exists.");
        return ToDto(user);
    }

    /// <summary>Changes the password and ends every session, so the learner logs in again with the new one.</summary>
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId, ct) ?? throw new UnauthorizedException("Account no longer exists.");
        if (!passwords.Verify(user.PasswordHash, request.CurrentPassword))
            throw new RequestValidationException("The current password is not correct.");
        ValidatePassword(request.NewPassword, user.Email);

        user.PasswordHash = passwords.Hash(request.NewPassword);
        await users.RevokeAllRefreshTokensAsync(userId, clock.UtcNow, ct);
        await users.SaveChangesAsync(ct);
    }

    /// <summary>Deletes the account together with its progress. The password confirms that the owner asked for it.</summary>
    public async Task DeleteAsync(Guid userId, string password, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId, ct) ?? throw new UnauthorizedException("Account no longer exists.");
        if (!passwords.Verify(user.PasswordHash, password))
            throw new RequestValidationException("The password is not correct.");

        await progress.ClearAsync(userId, ct);
        await users.DeleteAsync(user, ct);
    }

    private async Task<AuthResult> IssueAsync(User user, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var access = tokens.CreateAccessToken(user);
        var (refresh, hash) = tokens.CreateRefreshToken();
        var expires = now + tokens.RefreshTokenLifetime;

        await users.AddRefreshTokenAsync(
            new RefreshToken { UserId = user.Id, TokenHash = hash, CreatedAt = now, ExpiresAt = expires }, ct);
        await users.SaveChangesAsync(ct);

        return new AuthResult(new AuthResponse(access.Token, access.ExpiresAt, ToDto(user)), refresh, expires);
    }

    private static UserDto ToDto(User u) => new(u.Id, u.Email, u.DisplayName, u.CreatedAt);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static void ValidatePassword(string password, string email)
    {
        if (password.All(char.IsDigit))
            throw new RequestValidationException("The password must not be only digits.");
        if (string.Equals(password, email, StringComparison.OrdinalIgnoreCase))
            throw new RequestValidationException("The password must not be the same as the email.");
    }
}
