using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;
using Microsoft.AspNetCore.Identity;

namespace InterviewPal.Infrastructure.Auth;

/// <summary>Passwords are hashed with ASP.NET Core's PasswordHasher: PBKDF2, a random salt per password, versioned format.</summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    private readonly User _placeholder = new() { Email = "x", DisplayName = "x", PasswordHash = "x" };
    private readonly string _dummyHash;

    public PasswordService()
    {
        _dummyHash = _hasher.HashPassword(_placeholder, Guid.NewGuid().ToString("N"));
    }

    public string Hash(string password) => _hasher.HashPassword(_placeholder, password);

    public bool Verify(string hash, string password) =>
        _hasher.VerifyHashedPassword(_placeholder, hash, password) != PasswordVerificationResult.Failed;

    public void WasteTime() => _hasher.VerifyHashedPassword(_placeholder, _dummyHash, "not the password");
}
