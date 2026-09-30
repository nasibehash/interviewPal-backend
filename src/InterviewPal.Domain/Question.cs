namespace InterviewPal.Domain;

public class Question
{
    /// <summary>Stable slug, e.g. "angular-signals-vs-observables". Used to upsert seeded content.</summary>
    public required string Id { get; set; }

    public required string TechnologySlug { get; set; }
    public Technology? Technology { get; set; }

    public Level Level { get; set; }
    public QuestionType Type { get; set; }

    public required string Text { get; set; }
    public string? CodeSnippet { get; set; }
    public string? CodeLanguage { get; set; }

    /// <summary>2-3 lines: what to actually say in the interview.</summary>
    public required string ShortAnswer { get; set; }

    /// <summary>Deeper explanation, may contain a small code example (markdown).</summary>
    public required string Explanation { get; set; }

    public string? CommonMistake { get; set; }
    public string? FollowUpQuestion { get; set; }

    public int EstimatedSeconds { get; set; } = 90;

    /// <summary>First version of the technology the question applies to, e.g. "17".</summary>
    public string? MinVersion { get; set; }

    /// <summary>Last version the answer is valid for; null means "still valid".</summary>
    public string? MaxVersion { get; set; }

    public List<string> Tags { get; set; } = [];
    public List<Choice> Choices { get; set; } = [];
}

public class Choice
{
    public int Id { get; set; }
    public required string QuestionId { get; set; }
    public Question? Question { get; set; }
    public int Order { get; set; }
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}
