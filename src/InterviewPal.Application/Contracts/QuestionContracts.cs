using System.ComponentModel.DataAnnotations;
using InterviewPal.Domain;

namespace InterviewPal.Application.Contracts;

public record TechnologyDto(
    string Slug,
    string Name,
    string? CurrentVersion,
    string? SupportedFrom,
    int QuestionCount,
    IReadOnlyDictionary<string, int> CountByLevel);

public record ChoiceDto(int Id, string Text);

/// <summary>A question as shown to the learner: never contains the answer.</summary>
public record QuestionDto(
    string Id,
    string Technology,
    string Level,
    string Type,
    string Text,
    string? CodeSnippet,
    string? CodeLanguage,
    int EstimatedSeconds,
    string? MinVersion,
    string? MaxVersion,
    IReadOnlyList<string> Tags,
    IReadOnlyList<ChoiceDto> Choices);

/// <summary>Everything the learner learns once the question has been answered.</summary>
public record AnswerDto(
    IReadOnlyList<int> CorrectChoiceIds,
    string ShortAnswer,
    string Explanation,
    string? CommonMistake,
    string? FollowUpQuestion);

public record QuestionDetailDto(QuestionDto Question, AnswerDto Answer);

public record ReportQuestionRequest
{
    [Required]
    public ReportReason? Reason { get; init; }

    [MaxLength(1000)]
    public string? Message { get; init; }
}
