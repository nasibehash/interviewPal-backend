using System.Text.RegularExpressions;
using InterviewPal.Domain;

namespace InterviewPal.Infrastructure.Lessons;

public static partial class LessonValidator
{
    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugPattern();

    /// <summary>Returns human-readable problems; empty means the lesson is valid.</summary>
    public static IReadOnlyList<string> Validate(
        LessonFile lesson, string directory, IReadOnlyCollection<string> requiredTechnologies)
    {
        var problems = new List<string>();
        void Fail(string message) => problems.Add($"[{lesson.Id}] {message}");

        if (!SlugPattern().IsMatch(lesson.Id)) Fail("id must be a lowercase slug");
        if (Path.GetFileName(directory) != lesson.Id) Fail($"directory name '{Path.GetFileName(directory)}' must equal the id");
        if (!Enum.TryParse<LessonKind>(lesson.Kind, out var kind)) Fail($"invalid kind '{lesson.Kind}'");
        if (!Enum.TryParse<Level>(lesson.Level, out _)) Fail($"invalid level '{lesson.Level}'");
        if (lesson.Tags.Count == 0) Fail("at least one tag is required");
        if (lesson.EstimatedMinutes is < 3 or > 60) Fail("estimatedMinutes must be between 3 and 60");
        if (kind == LessonKind.Algorithm &&
            (string.IsNullOrWhiteSpace(lesson.TimeComplexity) || string.IsNullOrWhiteSpace(lesson.SpaceComplexity)))
            Fail("algorithms need timeComplexity and spaceComplexity");

        var missing = requiredTechnologies.Except(lesson.Implementations.Keys).Order().ToList();
        if (missing.Count > 0) Fail($"missing implementations for: {string.Join(", ", missing)}");
        var unknown = lesson.Implementations.Keys.Except(requiredTechnologies).Order().ToList();
        if (unknown.Count > 0) Fail($"unknown technologies: {string.Join(", ", unknown)}");

        foreach (var (slug, impl) in lesson.Implementations)
        {
            var file = Path.GetFileName(impl.File);
            if (file != impl.File || !File.Exists(Path.Combine(directory, file)))
                Fail($"{slug}: code file '{impl.File}' was not found next to lesson.json");
            else if (string.IsNullOrWhiteSpace(File.ReadAllText(Path.Combine(directory, file))))
                Fail($"{slug}: code file '{impl.File}' is empty");
        }

        if (lesson.Exercises.Count < 2) Fail("at least 2 exercises are required");
        var ids = new HashSet<string>();
        foreach (var e in lesson.Exercises)
        {
            if (!ids.Add(e.Id)) Fail($"duplicate exercise id '{e.Id}'");
            if (e.Choices.Count is < 2 or > 6) Fail($"exercise '{e.Id}' needs between 2 and 6 choices");
            if (e.Choices.Count(c => c.IsCorrect) != 1) Fail($"exercise '{e.Id}' needs exactly one correct choice");
        }

        return problems;
    }
}
