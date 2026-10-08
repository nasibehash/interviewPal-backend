namespace InterviewPal.Domain;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Lowercase, trimmed. Unique; this is what the learner logs in with.</summary>
    public required string Email { get; set; }

    public required string DisplayName { get; set; }

    /// <summary>PBKDF2 hash produced by the password hasher; the password itself is never stored.</summary>
    public required string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Wrong passwords since the last successful login (or the last lockout).</summary>
    public int FailedLoginCount { get; set; }

    /// <summary>While in the future, login is refused even with the right password.</summary>
    public DateTime? LockoutEnd { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
}

/// <summary>
/// A long-lived login. Only the SHA-256 hash of the token is stored, so a leaked database cannot be used to log in.
/// Tokens are rotated on every refresh: the old one is revoked and points to its replacement.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public required string TokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;
}
