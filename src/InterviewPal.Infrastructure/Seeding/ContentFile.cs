namespace InterviewPal.Infrastructure.Seeding;

/// <summary>Shape of the JSON files in /content (one file per technology).</summary>
public class ContentFile
{
    public required ContentTechnology Technology { get; set; }
    public List<ContentQuestion> Questions { get; set; } = [];
}

public class ContentTechnology
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public string? CurrentVersion { get; set; }
    public string? SupportedFrom { get; set; }
}

public class ContentQuestion
{
    public required string Id { get; set; }
    public required string Level { get; set; }
    public required string Type { get; set; }
    public required string Text { get; set; }
    public string? CodeSnippet { get; set; }
    public string? CodeLanguage { get; set; }
    public required string ShortAnswer { get; set; }
    public required string Explanation { get; set; }
    public string? CommonMistake { get; set; }
    public string? FollowUp { get; set; }
    public int EstimatedSeconds { get; set; } = 90;
    public string? MinVersion { get; set; }
    public string? MaxVersion { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<ContentChoice> Choices { get; set; } = [];
}

public class ContentChoice
{
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
}
