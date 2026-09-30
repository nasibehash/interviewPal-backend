using InterviewPal.Domain;

namespace InterviewPal.Infrastructure.Seeding;

public static class ContentValidator
{
    /// <summary>Returns a list of human-readable problems; empty means the content is valid.</summary>
    public static IReadOnlyList<string> Validate(ContentFile file)
    {
        var problems = new List<string>();
        var slug = file.Technology.Slug;
        var seen = new HashSet<string>();

        foreach (var q in file.Questions)
        {
            void Fail(string message) => problems.Add($"[{slug}/{q.Id}] {message}");

            if (!seen.Add(q.Id)) Fail("duplicate id");
            if (!q.Id.StartsWith(slug + "-")) Fail($"id must start with '{slug}-'");
            if (!Enum.TryParse<Level>(q.Level, out _)) Fail($"invalid level '{q.Level}'");
            if (!Enum.TryParse<QuestionType>(q.Type, out var type)) { Fail($"invalid type '{q.Type}'"); continue; }
            if (q.Tags.Count == 0) Fail("at least one tag is required");
            if (q.EstimatedSeconds is < 15 or > 600) Fail("estimatedSeconds must be between 15 and 600");
            if (type == QuestionType.CodeOutput && string.IsNullOrWhiteSpace(q.CodeSnippet))
                Fail("CodeOutput questions need a codeSnippet");

            var hasChoices = type is QuestionType.MultipleChoice or QuestionType.CodeOutput;
            if (hasChoices)
            {
                if (q.Choices.Count is < 2 or > 6) Fail("needs between 2 and 6 choices");
                if (q.Choices.Count(c => c.IsCorrect) != 1) Fail("needs exactly one correct choice");
            }
            else if (q.Choices.Count > 0)
            {
                Fail("ShortAnswer/Conceptual questions must not have choices");
            }
        }

        return problems;
    }
}
