using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InterviewPal.Domain;
using InterviewPal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InterviewPal.Infrastructure.Seeding;

/// <summary>
/// Loads the question bank from JSON files into the database. Questions are upserted by their stable id;
/// unchanged questions (same content hash) are left alone so choice ids stay stable between restarts.
/// </summary>
public class ContentSeeder(AppDbContext db, ILogger<ContentSeeder> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IReadOnlyList<ContentFile> LoadFiles(string directory)
    {
        if (!Directory.Exists(directory))
            throw new DirectoryNotFoundException($"Content directory '{directory}' was not found.");

        return Directory.GetFiles(directory, "*.json").Order()
            .Select(path => JsonSerializer.Deserialize<ContentFile>(File.ReadAllText(path), JsonOptions)
                            ?? throw new InvalidDataException($"'{path}' is empty."))
            .ToList();
    }

    public async Task SeedAsync(string directory, CancellationToken ct = default)
    {
        var files = LoadFiles(directory);

        var problems = files.SelectMany(ContentValidator.Validate).ToList();
        if (problems.Count > 0)
            throw new InvalidDataException("Invalid question content:\n" + string.Join("\n", problems));

        foreach (var file in files)
            await SeedTechnologyAsync(file, ct);

        var slugs = files.Select(f => f.Technology.Slug).ToList();
        var orphaned = await db.Technologies.Where(t => !slugs.Contains(t.Slug)).ToListAsync(ct);
        db.Technologies.RemoveRange(orphaned);
        await db.SaveChangesAsync(ct);
    }

    private async Task SeedTechnologyAsync(ContentFile file, CancellationToken ct)
    {
        var tech = await db.Technologies.FindAsync([file.Technology.Slug], ct);
        if (tech is null)
        {
            tech = new Technology { Slug = file.Technology.Slug, Name = file.Technology.Name };
            db.Technologies.Add(tech);
        }

        tech.Name = file.Technology.Name;
        tech.CurrentVersion = file.Technology.CurrentVersion;
        tech.SupportedFrom = file.Technology.SupportedFrom;

        var existing = await db.Questions.Include(q => q.Choices)
            .Where(q => q.TechnologySlug == tech.Slug).ToDictionaryAsync(q => q.Id, ct);

        int added = 0, updated = 0;
        foreach (var source in file.Questions)
        {
            var hash = Hash(source);
            if (existing.Remove(source.Id, out var current))
            {
                if (current.ContentHash == hash) continue;
                db.Choices.RemoveRange(current.Choices);
                current.Choices.Clear();
                Apply(current, source, hash);
                updated++;
            }
            else
            {
                var question = new Question
                {
                    Id = source.Id,
                    TechnologySlug = tech.Slug,
                    Text = source.Text,
                    ShortAnswer = source.ShortAnswer,
                    Explanation = source.Explanation
                };
                Apply(question, source, hash);
                db.Questions.Add(question);
                added++;
            }
        }

        // whatever is left was removed from the content files
        db.Questions.RemoveRange(existing.Values);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seeded {Technology}: {Added} added, {Updated} updated, {Removed} removed",
            tech.Slug, added, updated, existing.Count);
    }

    private static void Apply(Question q, ContentQuestion s, string hash)
    {
        q.Level = Enum.Parse<Level>(s.Level);
        q.Type = Enum.Parse<QuestionType>(s.Type);
        q.Text = s.Text;
        q.CodeSnippet = s.CodeSnippet;
        q.CodeLanguage = s.CodeLanguage;
        q.ShortAnswer = s.ShortAnswer;
        q.Explanation = s.Explanation;
        q.CommonMistake = s.CommonMistake;
        q.FollowUpQuestion = s.FollowUp;
        q.EstimatedSeconds = s.EstimatedSeconds;
        q.MinVersion = s.MinVersion;
        q.MaxVersion = s.MaxVersion;
        q.Tags = s.Tags.Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList();
        q.ContentHash = hash;
        q.Choices = s.Choices
            .Select((c, i) => new Choice { QuestionId = q.Id, Order = i, Text = c.Text, IsCorrect = c.IsCorrect })
            .ToList();
    }

    private static string Hash(ContentQuestion q) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(q))));
}
