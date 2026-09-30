namespace InterviewPal.Domain;

public class Technology
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public string? CurrentVersion { get; set; }
    public List<Question> Questions { get; set; } = [];
}
