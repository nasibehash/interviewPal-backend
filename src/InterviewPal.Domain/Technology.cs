namespace InterviewPal.Domain;

public class Technology
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public string? CurrentVersion { get; set; }

    /// <summary>Oldest version the question bank covers (the bank spans SupportedFrom..CurrentVersion).</summary>
    public string? SupportedFrom { get; set; }
    public List<Question> Questions { get; set; } = [];
}
