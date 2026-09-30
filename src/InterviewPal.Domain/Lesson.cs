namespace InterviewPal.Domain;

public enum LessonKind
{
    Algorithm,
    DesignPattern
}

/// <summary>
/// A short course on one algorithm or design pattern: a real-world scenario, the explanation, one implementation
/// per technology (written in that technology's idiom) and a few exercises.
/// </summary>
public class Lesson
{
    public required string Id { get; init; }
    public LessonKind Kind { get; init; }
    public required string Title { get; init; }
    public required string Category { get; init; }
    public Level Level { get; init; }
    public required string Summary { get; init; }
    public required string Scenario { get; init; }
    public required string Explanation { get; init; }
    public string? TimeComplexity { get; init; }
    public string? SpaceComplexity { get; init; }
    public required string WhenToUse { get; init; }
    public string? WhenNotToUse { get; init; }
    public string? CommonMistake { get; init; }
    public int EstimatedMinutes { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyDictionary<string, LessonImplementation> Implementations { get; init; } =
        new Dictionary<string, LessonImplementation>();
    public IReadOnlyList<LessonExercise> Exercises { get; init; } = [];
}

public class LessonImplementation
{
    public required string Technology { get; init; }
    public required string Title { get; init; }
    public required string Language { get; init; }
    public required string Code { get; init; }
    public required string Walkthrough { get; init; }
}

public class LessonExercise
{
    public required string Id { get; init; }
    public required string Text { get; init; }
    public required string Explanation { get; init; }

    /// <summary>Choices in display order; a choice id is its index.</summary>
    public IReadOnlyList<LessonChoice> Choices { get; init; } = [];
}

public class LessonChoice
{
    public int Id { get; init; }
    public required string Text { get; init; }
    public bool IsCorrect { get; init; }
}
