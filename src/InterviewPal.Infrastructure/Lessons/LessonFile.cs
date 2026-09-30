namespace InterviewPal.Infrastructure.Lessons;

/// <summary>Shape of <c>lesson.json</c>. The code of every implementation lives in its own file next to it.</summary>
public class LessonFile
{
    public required string Id { get; set; }
    public required string Kind { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    public required string Level { get; set; }
    public required string Summary { get; set; }
    public required string Scenario { get; set; }
    public required string Explanation { get; set; }
    public string? TimeComplexity { get; set; }
    public string? SpaceComplexity { get; set; }
    public required string WhenToUse { get; set; }
    public string? WhenNotToUse { get; set; }
    public string? CommonMistake { get; set; }
    public int EstimatedMinutes { get; set; } = 10;
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, LessonImplementationFile> Implementations { get; set; } = [];
    public List<LessonExerciseFile> Exercises { get; set; } = [];
}

public class LessonImplementationFile
{
    public required string Title { get; set; }

    /// <summary>Code file next to lesson.json, e.g. <c>react.tsx</c>.</summary>
    public required string File { get; set; }

    public required string Walkthrough { get; set; }
}

public class LessonExerciseFile
{
    public required string Id { get; set; }
    public required string Text { get; set; }
    public required string Explanation { get; set; }
    public List<LessonChoiceFile> Choices { get; set; } = [];
}

public class LessonChoiceFile
{
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}
