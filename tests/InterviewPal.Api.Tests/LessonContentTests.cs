using InterviewPal.Domain;
using InterviewPal.Infrastructure.Lessons;

namespace InterviewPal.Api.Tests;

/// <summary>Guards the real lessons in /content/lessons.</summary>
public class LessonContentTests
{
    private static readonly string[] Technologies = ["angular", "dotnet", "javascript", "nextjs", "react", "typescript"];

    private static string LessonsDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "content")))
            dir = dir.Parent;
        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("content folder not found"), "content", "lessons");
    }

    private static IReadOnlyList<Lesson> Lessons() => LessonCatalog.Read(LessonsDirectory(), Technologies);

    [Fact]
    public void Lessons_are_valid_and_implemented_for_all_six_technologies()
    {
        var lessons = Lessons();
        Assert.NotEmpty(lessons);
        Assert.All(lessons, l => Assert.Equal(Technologies, l.Implementations.Keys.Order()));
    }

    [Fact]
    public void There_are_enough_algorithms_and_design_patterns()
    {
        var lessons = Lessons();
        Assert.True(lessons.Count(l => l.Kind == LessonKind.Algorithm) >= 12);
        Assert.True(lessons.Count(l => l.Kind == LessonKind.DesignPattern) >= 10);
    }

    [Fact]
    public void Every_implementation_is_real_code_with_a_walkthrough()
    {
        foreach (var lesson in Lessons())
        foreach (var impl in lesson.Implementations.Values)
        {
            Assert.True(impl.Code.Split('\n').Length >= 8, $"{lesson.Id}/{impl.Technology} is too short to be a real example");
            Assert.True(impl.Walkthrough.Length >= 60, $"{lesson.Id}/{impl.Technology} needs a walkthrough");
            Assert.NotEqual("text", impl.Language);
        }
    }

    [Fact]
    public void Correct_choice_is_not_predictable_by_length_or_position()
    {
        var exercises = Lessons().SelectMany(l => l.Exercises).ToList();
        var longest = exercises.Count(e => e.Choices.Single(c => c.IsCorrect).Text.Length == e.Choices.Max(c => c.Text.Length)
                                           && e.Choices.Count(c => c.Text.Length == e.Choices.Max(x => x.Text.Length)) == 1);
        Assert.True(longest <= exercises.Count * 0.45, $"the correct choice is the unique longest in {longest}/{exercises.Count} exercises");

        var first = exercises.Count(e => e.Choices[0].IsCorrect);
        Assert.True(first <= exercises.Count * 0.45, $"the correct choice is first in {first}/{exercises.Count} exercises");
    }
}
