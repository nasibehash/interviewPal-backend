using System.ComponentModel.DataAnnotations;

namespace InterviewPal.Application.Contracts;

public record HistoryEntryDto(
    Guid Id,
    DateTime At,
    string Mode,
    int Total,
    int Correct,
    int Percent,
    IReadOnlyList<ScoreBreakdown> ByTechnology);

public record QuestionStatDto(
    string QuestionId,
    string Technology,
    string Level,
    string Text,
    IReadOnlyList<string> Tags,
    int Seen,
    int Correct,
    bool LastCorrect,
    DateTime LastAt);

public record LessonProgressDto(string LessonId, int Total, IReadOnlyDictionary<string, bool> Answers);

public record ProgressDto(
    IReadOnlyList<HistoryEntryDto> History,
    IReadOnlyList<QuestionStatDto> QuestionStats,
    IReadOnlyList<LessonProgressDto> Lessons);

/// <summary>Progress collected in the browser before the learner had an account.</summary>
public record ImportProgressRequest
{
    [MaxLength(200)]
    public List<ImportHistoryEntry> History { get; init; } = [];

    [MaxLength(400)]
    public List<ImportQuestionStat> QuestionStats { get; init; } = [];

    [MaxLength(100)]
    public List<ImportLessonProgress> Lessons { get; init; } = [];
}

public record ImportHistoryEntry
{
    public DateTime At { get; init; }
    public string Mode { get; init; } = "";

    [Range(1, 100)]
    public int Total { get; init; }

    [Range(0, 100)]
    public int Correct { get; init; }

    public List<ScoreBreakdown> ByTechnology { get; init; } = [];
}

public record ImportQuestionStat
{
    public string QuestionId { get; init; } = "";

    [Range(1, 10_000)]
    public int Seen { get; init; }

    [Range(0, 10_000)]
    public int Correct { get; init; }

    public bool LastCorrect { get; init; }
    public DateTime LastAt { get; init; }
}

public record ImportLessonProgress
{
    public string LessonId { get; init; } = "";
    public Dictionary<string, bool> Answers { get; init; } = [];
}
