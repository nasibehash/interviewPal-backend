using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

public class LessonService(ILessonCatalog catalog)
{
    public IReadOnlyList<LessonSummaryDto> List(string? kind, string? level, string? category, string? technology)
    {
        LessonKind? parsedKind = null;
        if (!string.IsNullOrWhiteSpace(kind))
        {
            if (!Enum.TryParse<LessonKind>(kind, ignoreCase: true, out var k))
                throw new RequestValidationException($"Unknown kind '{kind}'. Use Algorithm or DesignPattern.");
            parsedKind = k;
        }

        Level? parsedLevel = null;
        if (!string.IsNullOrWhiteSpace(level))
        {
            if (!Enum.TryParse<Level>(level, ignoreCase: true, out var l))
                throw new RequestValidationException($"Unknown level '{level}'. Use Junior, Mid or Senior.");
            parsedLevel = l;
        }

        return catalog.All
            .Where(l => parsedKind is null || l.Kind == parsedKind)
            .Where(l => parsedLevel is null || l.Level == parsedLevel)
            .Where(l => string.IsNullOrWhiteSpace(category) || l.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .Where(l => string.IsNullOrWhiteSpace(technology) || l.Implementations.ContainsKey(technology.ToLowerInvariant()))
            .Select(ToSummary)
            .ToList();
    }

    public LessonDetailDto Get(string id, string? technology)
    {
        var lesson = Find(id);
        var technologies = lesson.Implementations.Keys.Order().ToList();

        LessonImplementationDto? implementation = null;
        if (!string.IsNullOrWhiteSpace(technology))
        {
            if (!lesson.Implementations.TryGetValue(technology.ToLowerInvariant(), out var impl))
                throw new RequestValidationException(
                    $"Lesson '{id}' has no implementation for '{technology}'. Available: {string.Join(", ", technologies)}.");
            implementation = new LessonImplementationDto(impl.Technology, impl.Title, impl.Language, impl.Code, impl.Walkthrough);
        }

        return new LessonDetailDto(
            lesson.Id, lesson.Kind.ToString(), lesson.Title, lesson.Category, lesson.Level.ToString(), lesson.Summary,
            lesson.Scenario, lesson.Explanation, lesson.TimeComplexity, lesson.SpaceComplexity, lesson.WhenToUse,
            lesson.WhenNotToUse, lesson.CommonMistake, lesson.EstimatedMinutes, lesson.Tags, technologies, implementation,
            lesson.Exercises.Select(e => new LessonExerciseDto(e.Id, e.Text,
                e.Choices.Select(c => new ChoiceDto(c.Id, c.Text)).ToList())).ToList());
    }

    public CheckExerciseResult Check(string id, string exerciseId, CheckExerciseRequest request)
    {
        var exercise = Find(id).Exercises.FirstOrDefault(e => e.Id == exerciseId)
                       ?? throw new NotFoundException($"Exercise '{exerciseId}' was not found in lesson '{id}'.");
        var choice = exercise.Choices.FirstOrDefault(c => c.Id == request.ChoiceId)
                     ?? throw new RequestValidationException($"Choice {request.ChoiceId} does not exist.");
        var correct = exercise.Choices.Single(c => c.IsCorrect);
        return new CheckExerciseResult(exercise.Id, choice.IsCorrect, correct.Id, exercise.Explanation);
    }

    private Lesson Find(string id) =>
        catalog.All.FirstOrDefault(l => l.Id == id) ?? throw new NotFoundException($"Lesson '{id}' was not found.");

    private static LessonSummaryDto ToSummary(Lesson l) =>
        new(l.Id, l.Kind.ToString(), l.Title, l.Category, l.Level.ToString(), l.Summary, l.Tags, l.EstimatedMinutes);
}
