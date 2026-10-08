namespace InterviewPal.Domain;

/// <summary>One finished practice session of a user.</summary>
public class PracticeHistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime At { get; set; }
    public PracticeModeKind Mode { get; set; }
    public int Total { get; set; }
    public int Correct { get; set; }
    public int Percent { get; set; }

    /// <summary>Score per technology as JSON: [{ key, total, correct, percent }].</summary>
    public string ByTechnologyJson { get; set; } = "[]";
}

public enum PracticeModeKind
{
    Learning = 1,
    Interview = 2,
    Flashcard = 3
}

/// <summary>How a user has been doing on one question. Composite key (UserId, QuestionId).</summary>
public class QuestionStat
{
    public Guid UserId { get; set; }
    public required string QuestionId { get; set; }
    public int Seen { get; set; }
    public int Correct { get; set; }
    public bool LastCorrect { get; set; }
    public DateTime LastAt { get; set; }
}

/// <summary>The latest answer of a user to one exercise of a lesson. Composite key (UserId, LessonId, ExerciseId).</summary>
public class LessonExerciseProgress
{
    public Guid UserId { get; set; }
    public required string LessonId { get; set; }
    public required string ExerciseId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime AnsweredAt { get; set; }
}
