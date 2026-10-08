using InterviewPal.Domain;

namespace InterviewPal.Application.Abstractions;

public interface IProgressRepository
{
    Task AddHistoryAsync(PracticeHistoryEntry entry, CancellationToken ct);

    Task<IReadOnlyList<PracticeHistoryEntry>> GetRecentHistoryAsync(Guid userId, int count, CancellationToken ct);

    Task<bool> HistoryExistsAsync(Guid userId, DateTime at, PracticeModeKind mode, CancellationToken ct);

    Task<Dictionary<string, QuestionStat>> GetStatsAsync(Guid userId, IReadOnlyCollection<string>? questionIds, CancellationToken ct);

    void AddStat(QuestionStat stat);

    Task<IReadOnlyList<LessonExerciseProgress>> GetLessonProgressAsync(Guid userId, CancellationToken ct);

    Task UpsertLessonProgressAsync(LessonExerciseProgress progress, bool overwrite, CancellationToken ct);

    /// <summary>Removes history, stats and lesson progress of the user (the account stays).</summary>
    Task ClearAsync(Guid userId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
