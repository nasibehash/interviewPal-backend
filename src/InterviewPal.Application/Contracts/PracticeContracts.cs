using System.ComponentModel.DataAnnotations;

namespace InterviewPal.Application.Contracts;

public enum PracticeMode
{
    /// <summary>Explanation is shown right after each answer.</summary>
    Learning = 1,

    /// <summary>Timed, result only at the end.</summary>
    Interview = 2,

    /// <summary>Quick review: learner marks "knew it" / "didn't know it".</summary>
    Flashcard = 3
}

public record StartPracticeRequest
{
    /// <summary>Technology slugs. Empty means all technologies.</summary>
    public List<string> Technologies { get; init; } = [];

    /// <summary>Levels (Junior/Mid/Senior). Empty means all levels.</summary>
    public List<string> Levels { get; init; } = [];

    [Range(5, 100)]
    public int Count { get; init; } = 20;

    public PracticeMode Mode { get; init; } = PracticeMode.Learning;

    /// <summary>Optional seed for a reproducible selection.</summary>
    public int? Seed { get; init; }
}

public record PracticeSessionDto(
    PracticeMode Mode,
    int RequestedCount,
    int TotalQuestions,
    int EstimatedMinutes,
    int? TimeLimitSeconds,
    IReadOnlyList<QuestionDto> Questions);

public record CheckAnswerRequest
{
    /// <summary>Selected choice for multiple-choice and code-output questions.</summary>
    public int? ChoiceId { get; init; }

    /// <summary>Self-assessment for short-answer, conceptual questions and flashcards.</summary>
    public bool? KnewIt { get; init; }
}

public record CheckAnswerResult(
    string QuestionId,
    bool IsCorrect,
    AnswerDto Answer);

public record SubmittedAnswer
{
    [Required]
    public string QuestionId { get; init; } = "";
    public int? ChoiceId { get; init; }
    public bool? KnewIt { get; init; }
}

public record EvaluatePracticeRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public List<SubmittedAnswer> Answers { get; init; } = [];

    /// <summary>Mode of the session, stored in the history of a logged-in learner.</summary>
    public PracticeMode Mode { get; init; } = PracticeMode.Learning;
}

public record ScoreBreakdown(string Key, int Total, int Correct, int Percent);

public record EvaluationResult(
    int Total,
    int Correct,
    int Percent,
    IReadOnlyList<ScoreBreakdown> ByTechnology,
    IReadOnlyList<ScoreBreakdown> ByLevel,
    IReadOnlyList<ScoreBreakdown> WeakTags,
    IReadOnlyList<string> WeakQuestionIds,
    IReadOnlyList<CheckAnswerResult> Results);
