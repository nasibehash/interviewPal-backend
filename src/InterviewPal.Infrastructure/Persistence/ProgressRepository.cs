using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Infrastructure.Persistence;

public class ProgressRepository(AppDbContext db) : IProgressRepository
{
    public Task AddHistoryAsync(PracticeHistoryEntry entry, CancellationToken ct)
    {
        db.PracticeHistory.Add(entry);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PracticeHistoryEntry>> GetRecentHistoryAsync(Guid userId, int count, CancellationToken ct) =>
        await db.PracticeHistory.AsNoTracking().Where(h => h.UserId == userId)
            .OrderByDescending(h => h.At).Take(count).ToListAsync(ct);

    public Task<bool> HistoryExistsAsync(Guid userId, DateTime at, PracticeModeKind mode, CancellationToken ct) =>
        db.PracticeHistory.AnyAsync(h => h.UserId == userId && h.At == at && h.Mode == mode, ct);

    public async Task<Dictionary<string, QuestionStat>> GetStatsAsync(
        Guid userId, IReadOnlyCollection<string>? questionIds, CancellationToken ct)
    {
        var query = db.QuestionStats.Where(s => s.UserId == userId);
        if (questionIds is not null) query = query.Where(s => questionIds.Contains(s.QuestionId));
        return await query.ToDictionaryAsync(s => s.QuestionId, ct);
    }

    public void AddStat(QuestionStat stat) => db.QuestionStats.Add(stat);

    public async Task<IReadOnlyList<LessonExerciseProgress>> GetLessonProgressAsync(Guid userId, CancellationToken ct) =>
        await db.LessonProgress.AsNoTracking().Where(p => p.UserId == userId).ToListAsync(ct);

    public async Task UpsertLessonProgressAsync(LessonExerciseProgress progress, bool overwrite, CancellationToken ct)
    {
        var existing = await db.LessonProgress.FindAsync([progress.UserId, progress.LessonId, progress.ExerciseId], ct);
        if (existing is null)
        {
            db.LessonProgress.Add(progress);
        }
        else if (overwrite)
        {
            existing.IsCorrect = progress.IsCorrect;
            existing.AnsweredAt = progress.AnsweredAt;
        }
    }

    public async Task ClearAsync(Guid userId, CancellationToken ct)
    {
        await db.PracticeHistory.Where(h => h.UserId == userId).ExecuteDeleteAsync(ct);
        await db.QuestionStats.Where(s => s.UserId == userId).ExecuteDeleteAsync(ct);
        await db.LessonProgress.Where(p => p.UserId == userId).ExecuteDeleteAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
