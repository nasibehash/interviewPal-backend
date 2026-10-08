using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Infrastructure.Persistence;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // two registrations with the same email at the same moment: the unique index decides
            db.Entry(user).State = EntityState.Detached;
            throw new Application.Services.ConflictException("An account with this email already exists.");
        }
    }

    public async Task DeleteAsync(User user, CancellationToken ct)
    {
        db.Users.Remove(user); // history, stats, lesson progress and refresh tokens go with it (cascade)
        await db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct)
    {
        db.RefreshTokens.Add(token);
        return Task.CompletedTask;
    }

    public Task RevokeAllRefreshTokensAsync(Guid userId, DateTime now, CancellationToken ct) =>
        db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
