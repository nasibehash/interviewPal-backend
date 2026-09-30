namespace InterviewPal.Domain;

public class QuestionReport
{
    public int Id { get; set; }
    public required string QuestionId { get; set; }
    public Question? Question { get; set; }
    public ReportReason Reason { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
