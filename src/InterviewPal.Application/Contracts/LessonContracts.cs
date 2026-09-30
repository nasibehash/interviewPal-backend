using System.ComponentModel.DataAnnotations;

namespace InterviewPal.Application.Contracts;

public record LessonSummaryDto(
    string Id,
    string Kind,
    string Title,
    string Category,
    string Level,
    string Summary,
    IReadOnlyList<string> Tags,
    int EstimatedMinutes);

public record LessonImplementationDto(string Technology, string Title, string Language, string Code, string Walkthrough);

/// <summary>An exercise as shown to the learner: it never says which choice is right.</summary>
public record LessonExerciseDto(string Id, string Text, IReadOnlyList<ChoiceDto> Choices);

public record LessonDetailDto(
    string Id,
    string Kind,
    string Title,
    string Category,
    string Level,
    string Summary,
    string Scenario,
    string Explanation,
    string? TimeComplexity,
    string? SpaceComplexity,
    string WhenToUse,
    string? WhenNotToUse,
    string? CommonMistake,
    int EstimatedMinutes,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Technologies,
    LessonImplementationDto? Implementation,
    IReadOnlyList<LessonExerciseDto> Exercises);

public record CheckExerciseRequest
{
    [Required]
    public int? ChoiceId { get; init; }
}

public record CheckExerciseResult(string ExerciseId, bool IsCorrect, int CorrectChoiceId, string Explanation);
