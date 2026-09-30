using InterviewPal.Infrastructure.Seeding;

namespace InterviewPal.Api.Tests;

/// <summary>Guards the real question bank in /content.</summary>
public class ContentTests
{
    private static string ContentDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "content")))
            dir = dir.Parent;
        return Path.Combine(dir?.FullName ?? throw new DirectoryNotFoundException("content folder not found"), "content");
    }

    [Fact]
    public void Content_files_are_valid()
    {
        var files = ContentSeeder.LoadFiles(ContentDirectory());
        var problems = files.SelectMany(ContentValidator.Validate).ToList();
        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Question_ids_are_unique_across_files()
    {
        var ids = ContentSeeder.LoadFiles(ContentDirectory()).SelectMany(f => f.Questions.Select(q => q.Id)).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Every_technology_has_at_least_50_questions_across_all_levels()
    {
        foreach (var file in ContentSeeder.LoadFiles(ContentDirectory()))
        {
            Assert.True(file.Questions.Count >= 50, $"{file.Technology.Slug} has only {file.Questions.Count} questions");
            foreach (var level in new[] { "Junior", "Mid", "Senior" })
                Assert.Contains(file.Questions, q => q.Level == level);
        }
    }

    [Fact]
    public void Every_technology_declares_the_version_window_it_covers()
    {
        var files = ContentSeeder.LoadFiles(ContentDirectory());
        Assert.NotEmpty(files);
        foreach (var file in files)
        {
            Assert.False(string.IsNullOrWhiteSpace(file.Technology.CurrentVersion), $"{file.Technology.Slug} has no currentVersion");
            Assert.False(string.IsNullOrWhiteSpace(file.Technology.SupportedFrom), $"{file.Technology.Slug} has no supportedFrom");
        }
    }

    [Fact]
    public void Every_question_declares_the_version_it_applies_from()
    {
        foreach (var file in ContentSeeder.LoadFiles(ContentDirectory()))
            foreach (var q in file.Questions)
                Assert.False(string.IsNullOrWhiteSpace(q.MinVersion), $"{q.Id} has no minVersion");
    }
}
