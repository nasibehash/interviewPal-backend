using InterviewPal.Domain;

namespace InterviewPal.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct);
    Task<User?> FindByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);

    /// <summary>Deletes the user and everything that belongs to the user.</summary>
    Task DeleteAsync(User user, CancellationToken ct);

    Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash, CancellationToken ct);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct);
    Task RevokeAllRefreshTokensAsync(Guid userId, DateTime now, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

public interface IPasswordService
{
    string Hash(string password);

    /// <summary>True when <paramref name="password"/> matches <paramref name="hash"/>.</summary>
    bool Verify(string hash, string password);

    /// <summary>Spends the same time as a real check; used when the user does not exist so response time does not tell.</summary>
    void WasteTime();
}

public record IssuedAccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    IssuedAccessToken CreateAccessToken(User user);

    /// <summary>A new random refresh token and its hash (only the hash is stored).</summary>
    (string Token, string Hash) CreateRefreshToken();

    string HashRefreshToken(string token);

    TimeSpan RefreshTokenLifetime { get; }
}

/// <summary>The id of the user making the current request, or null for an anonymous request.</summary>
public interface ICurrentUser
{
    Guid? Id { get; }
}

public interface IClock
{
    DateTime UtcNow { get; }
}
