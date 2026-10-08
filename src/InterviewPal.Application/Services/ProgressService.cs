using System.Text.Json;
using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

/// <summary>Server-side copy of what the learner did: practice history, per-question stats and lesson exercises.</summary>
public class ProgressService(IProgressRepository progress, IQuestionRepository questions, ILessonCatalog lessons, IClock clock)
{
    private const int HistoryLimit = 100;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>Called when a logged-in learner finishes a practice session.</summary>
    public async Task RecordPracticeAsync(
        Guid userId, PracticeMode mode, EvaluationResult evaluation, CancellationToken ct)
    {
        var now = clock.UtcNow;
        await progress.AddHistoryAsync(new PracticeHistoryEntry
        {
            UserId = userId,
            At = now,
            Mode = ToKind(mode),
            Total = evaluation.Total,
            Correct = evaluation.Correct,
            Percent = evaluation.Percent,
            ByTechnologyJson = JsonSerializer.Serialize(evaluation.ByTechnology, Json)
        }, ct);

        var ids = evaluation.Results.Select(r => r.QuestionId).ToList();
        var stats = await progress.GetStatsAsync(userId, ids, ct);
        foreach (var result in evaluation.Results)
        {
            if (!stats.TryGetValue(result.QuestionId, out var stat))
            {
                stat = new QuestionStat { UserId = userId, QuestionId = result.QuestionId };
                progress.AddStat(stat);
            }

            stat.Seen++;
            if (result.IsCorrect) stat.Correct++;
            stat.LastCorrect = result.IsCorrect;
            stat.LastAt = now;
        }

        await progress.SaveChangesAsync(ct);
    }

    /// <summary>Called when a logged-in learner answers a lesson exercise (the latest answer is kept).</summary>
    public async Task RecordLessonAnswerAsync(Guid userId, string lessonId, string exerciseId, bool isCorrect, CancellationToken ct)
    {
        await progress.UpsertLessonProgressAsync(new LessonExerciseProgress
        {
            UserId = userId, LessonId = lessonId, ExerciseId = exerciseId, IsCorrect = isCorrect, AnsweredAt = clock.UtcNow
        }, overwrite: true, ct);
        await progress.SaveChangesAsync(ct);
    }

    public async Task<ProgressDto> GetAsync(Guid userId, CancellationToken ct)
    {
        var history = (await progress.GetRecentHistoryAsync(userId, HistoryLimit, ct))
            .Select(h => new HistoryEntryDto(
                h.Id, h.At, h.Mode.ToString(), h.Total, h.Correct, h.Percent,
                JsonSerializer.Deserialize<List<ScoreBreakdown>>(h.ByTechnologyJson, Json) ?? []))
            .ToList();

        var stats = await progress.GetStatsAsync(userId, null, ct);
        var known = (await questions.GetByIdsAsync(stats.Keys.ToList(), ct)).ToDictionary(q => q.Id);
        var statDtos = stats.Values
            .Where(s => known.ContainsKey(s.QuestionId)) // a question removed from the content has no text to show
            .OrderByDescending(s => s.LastAt)
            .Select(s =>
            {
                var q = known[s.QuestionId];
                return new QuestionStatDto(q.Id, q.TechnologySlug, q.Level.ToString(), q.Text, q.Tags, s.Seen, s.Correct, s.LastCorrect, s.LastAt);
            })
            .ToList();

        var totals = lessons.All.ToDictionary(l => l.Id, l => l.Exercises.Count);
        var lessonDtos = (await progress.GetLessonProgressAsync(userId, ct))
            .Where(p => totals.ContainsKey(p.LessonId))
            .GroupBy(p => p.LessonId)
            .Select(g => new LessonProgressDto(g.Key, totals[g.Key], g.ToDictionary(p => p.ExerciseId, p => p.IsCorrect)))
            .ToList();

        return new ProgressDto(history, statDtos, lessonDtos);
    }

    /// <summary>
    /// Adds what was collected in the browser before login. Safe to repeat: history entries are matched by time and
    /// mode, lesson answers the server already has are kept. Unknown questions and lessons are skipped.
    /// </summary>
    public async Task ImportAsync(Guid userId, ImportProgressRequest request, CancellationToken ct)
    {
        var now = clock.UtcNow;

        foreach (var entry in request.History.Where(h => h.Correct <= h.Total))
        {
            if (!Enum.TryParse<PracticeModeKind>(entry.Mode, ignoreCase: true, out var mode)) continue;
            var at = DateTime.SpecifyKind(entry.At, DateTimeKind.Utc);
            if (at > now.AddMinutes(5) || await progress.HistoryExistsAsync(userId, at, mode, ct)) continue;

            await progress.AddHistoryAsync(new PracticeHistoryEntry
            {
                UserId = userId, At = at, Mode = mode, Total = entry.Total, Correct = entry.Correct,
                Percent = (int)Math.Round(100.0 * entry.Correct / entry.Total),
                ByTechnologyJson = JsonSerializer.Serialize(entry.ByTechnology.Take(10), Json)
            }, ct);
        }

        var ids = request.QuestionStats.Select(s => s.QuestionId).Distinct().ToList();
        var existing = await progress.GetStatsAsync(userId, ids, ct);
        var valid = (await questions.GetByIdsAsync(ids, ct)).Select(q => q.Id).ToHashSet();
        foreach (var s in request.QuestionStats.Where(s => valid.Contains(s.QuestionId) && s.Correct <= s.Seen))
        {
            var at = DateTime.SpecifyKind(s.LastAt, DateTimeKind.Utc);
            if (!existing.TryGetValue(s.QuestionId, out var stat))
            {
                stat = new QuestionStat { UserId = userId, QuestionId = s.QuestionId, LastAt = DateTime.MinValue };
                existing[s.QuestionId] = stat;
                progress.AddStat(stat);
            }

            stat.Seen += s.Seen;
            stat.Correct += s.Correct;
            if (at >= stat.LastAt)
            {
                stat.LastAt = at;
                stat.LastCorrect = s.LastCorrect;
            }
        }

        var exercises = lessons.All.ToDictionary(l => l.Id, l => l.Exercises.Select(e => e.Id).ToHashSet());
        foreach (var lesson in request.Lessons.Where(l => exercises.ContainsKey(l.LessonId)))
        foreach (var (exerciseId, isCorrect) in lesson.Answers.Where(a => exercises[lesson.LessonId].Contains(a.Key)))
        {
            await progress.UpsertLessonProgressAsync(new LessonExerciseProgress
            {
                UserId = userId, LessonId = lesson.LessonId, ExerciseId = exerciseId, IsCorrect = isCorrect, AnsweredAt = now
            }, overwrite: false, ct);
        }

        await progress.SaveChangesAsync(ct);
    }

    public Task ClearAsync(Guid userId, CancellationToken ct) => progress.ClearAsync(userId, ct);

    private static PracticeModeKind ToKind(PracticeMode mode) => mode switch
    {
        PracticeMode.Interview => PracticeModeKind.Interview,
        PracticeMode.Flashcard => PracticeModeKind.Flashcard,
        _ => PracticeModeKind.Learning
    };
}
