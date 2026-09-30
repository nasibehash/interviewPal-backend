using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;

namespace InterviewPal.Infrastructure.Lessons;

/// <summary>
/// Lessons are static content, so they are loaded once at startup into memory instead of the database.
/// Layout: <c>content/lessons/&lt;lesson-id&gt;/lesson.json</c> plus one code file per technology.
/// </summary>
public class LessonCatalog : ILessonCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static readonly Dictionary<string, string> Languages = new()
    {
        [".js"] = "javascript",
        [".ts"] = "typescript",
        [".tsx"] = "typescript",
        [".cs"] = "csharp"
    };

    private IReadOnlyList<Lesson> _all = [];

    public IReadOnlyList<Lesson> All => _all;

    public void Load(string lessonsDirectory, IReadOnlyCollection<string> requiredTechnologies)
    {
        _all = Read(lessonsDirectory, requiredTechnologies);
    }

    public static IReadOnlyList<Lesson> Read(string lessonsDirectory, IReadOnlyCollection<string> requiredTechnologies)
    {
        if (!Directory.Exists(lessonsDirectory)) return [];

        var lessons = new List<Lesson>();
        var problems = new List<string>();

        foreach (var directory in Directory.GetDirectories(lessonsDirectory).Order())
        {
            var path = Path.Combine(directory, "lesson.json");
            if (!File.Exists(path))
            {
                problems.Add($"[{Path.GetFileName(directory)}] lesson.json is missing");
                continue;
            }

            var file = JsonSerializer.Deserialize<LessonFile>(File.ReadAllText(path), JsonOptions)
                       ?? throw new InvalidDataException($"'{path}' is empty.");
            var found = LessonValidator.Validate(file, directory, requiredTechnologies);
            problems.AddRange(found);
            if (found.Count == 0) lessons.Add(ToLesson(file, directory));
        }

        var duplicated = lessons.GroupBy(l => l.Id).Where(g => g.Count() > 1).Select(g => g.Key);
        problems.AddRange(duplicated.Select(id => $"[{id}] duplicate lesson id"));

        if (problems.Count > 0)
            throw new InvalidDataException("Invalid lesson content:\n" + string.Join("\n", problems));
        return lessons;
    }

    public static string LanguageOf(string fileName) =>
        Languages.GetValueOrDefault(Path.GetExtension(fileName).ToLowerInvariant(), "text");

    private static Lesson ToLesson(LessonFile f, string directory) => new()
    {
        Id = f.Id,
        Kind = Enum.Parse<LessonKind>(f.Kind),
        Title = f.Title,
        Category = f.Category,
        Level = Enum.Parse<Level>(f.Level),
        Summary = f.Summary,
        Scenario = f.Scenario,
        Explanation = f.Explanation,
        TimeComplexity = f.TimeComplexity,
        SpaceComplexity = f.SpaceComplexity,
        WhenToUse = f.WhenToUse,
        WhenNotToUse = f.WhenNotToUse,
        CommonMistake = f.CommonMistake,
        EstimatedMinutes = f.EstimatedMinutes,
        Tags = f.Tags.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList(),
        Implementations = f.Implementations.ToDictionary(
            kv => kv.Key,
            kv => new LessonImplementation
            {
                Technology = kv.Key,
                Title = kv.Value.Title,
                Language = LanguageOf(kv.Value.File),
                Code = File.ReadAllText(Path.Combine(directory, kv.Value.File)).ReplaceLineEndings("\n").TrimEnd(),
                Walkthrough = kv.Value.Walkthrough
            }),
        Exercises = f.Exercises.Select(e => new LessonExercise
        {
            Id = e.Id,
            Text = e.Text,
            Explanation = e.Explanation,
            Choices = Shuffle(f.Id + "/" + e.Id, e.Choices)
                .Select((c, i) => new LessonChoice { Id = i, Text = c.Text, IsCorrect = c.IsCorrect })
                .ToList()
        }).ToList()
    };

    /// <summary>
    /// Deterministic shuffle (seeded by the exercise id): authors do not have to balance the position of the correct
    /// choice, and it stays the same between restarts.
    /// </summary>
    private static List<LessonChoiceFile> Shuffle(string seed, List<LessonChoiceFile> choices)
    {
        var rng = new Random(BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(seed)), 0));
        var result = choices.ToList();
        for (var i = result.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }
}
