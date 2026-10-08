using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

public class PracticeService(IQuestionRepository questions, ProgressService progress, ICurrentUser currentUser)
{
    private const int WeakTagThresholdPercent = 60;

    public async Task<PracticeSessionDto> StartAsync(StartPracticeRequest request, CancellationToken ct)
    {
        var technologies = request.Technologies
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
        var levels = ParseLevels(request.Levels);

        var candidates = await questions.GetCandidatesAsync(technologies, levels, ct);
        var rng = request.Seed is { } seed ? new Random(seed) : new Random();
        var selected = SelectBalanced(candidates, request.Count, rng);

        var seconds = selected.Sum(q => q.EstimatedSeconds);
        return new PracticeSessionDto(
            request.Mode,
            request.Count,
            selected.Count,
            (int)Math.Ceiling(seconds / 60.0),
            request.Mode == PracticeMode.Interview ? seconds : null,
            selected.Select(QuestionMapper.ToDto).ToList());
    }

    public async Task<CheckAnswerResult> CheckAsync(string questionId, CheckAnswerRequest request, CancellationToken ct)
    {
        var q = await questions.GetAsync(questionId, ct)
                ?? throw new NotFoundException($"Question '{questionId}' was not found.");
        return Grade(q, request.ChoiceId, request.KnewIt);
    }

    public async Task<EvaluationResult> EvaluateAsync(EvaluatePracticeRequest request, CancellationToken ct)
    {
        var duplicate = request.Answers.GroupBy(a => a.QuestionId).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new RequestValidationException($"Question '{duplicate.Key}' was answered more than once.");

        var loaded = await questions.GetByIdsAsync(request.Answers.Select(a => a.QuestionId).ToList(), ct);
        var byId = loaded.ToDictionary(q => q.Id);

        var missing = request.Answers.FirstOrDefault(a => !byId.ContainsKey(a.QuestionId));
        if (missing is not null)
            throw new NotFoundException($"Question '{missing.QuestionId}' was not found.");

        var graded = request.Answers
            .Select(a => (Question: byId[a.QuestionId], Result: Grade(byId[a.QuestionId], a.ChoiceId, a.KnewIt)))
            .ToList();

        var total = graded.Count;
        var correct = graded.Count(g => g.Result.IsCorrect);

        var weakTags = graded
            .SelectMany(g => g.Question.Tags.Select(tag => (Tag: tag, g.Result.IsCorrect)))
            .GroupBy(x => x.Tag)
            .Select(g => Breakdown(g.Key, g.Count(), g.Count(x => x.IsCorrect)))
            .Where(b => b.Percent < WeakTagThresholdPercent)
            .OrderBy(b => b.Percent).ThenByDescending(b => b.Total)
            .ToList();

        var result = new EvaluationResult(
            total,
            correct,
            Percent(correct, total),
            graded.GroupBy(g => g.Question.TechnologySlug)
                .Select(g => Breakdown(g.Key, g.Count(), g.Count(x => x.Result.IsCorrect)))
                .OrderBy(b => b.Key).ToList(),
            graded.GroupBy(g => g.Question.Level)
                .OrderBy(g => g.Key)
                .Select(g => Breakdown(g.Key.ToString(), g.Count(), g.Count(x => x.Result.IsCorrect))).ToList(),
            weakTags,
            graded.Where(g => !g.Result.IsCorrect).Select(g => g.Question.Id).ToList(),
            graded.Select(g => g.Result).ToList());

        // a logged-in learner's result is kept on the server; an anonymous one is only returned
        if (currentUser.Id is { } userId)
            await progress.RecordPracticeAsync(userId, request.Mode, result, ct);

        return result;
    }

    private static CheckAnswerResult Grade(Question q, int? choiceId, bool? knewIt)
    {
        bool isCorrect;
        if (QuestionMapper.HasChoices(q))
        {
            if (choiceId is null)
                throw new RequestValidationException($"Question '{q.Id}' needs a choiceId.");
            var choice = q.Choices.FirstOrDefault(c => c.Id == choiceId)
                         ?? throw new RequestValidationException(
                             $"Choice {choiceId} does not belong to question '{q.Id}'.");
            isCorrect = choice.IsCorrect;
        }
        else
        {
            isCorrect = knewIt
                        ?? throw new RequestValidationException($"Question '{q.Id}' needs knewIt (true/false).");
        }

        return new CheckAnswerResult(q.Id, isCorrect, QuestionMapper.ToAnswer(q));
    }

    private static List<Level> ParseLevels(IEnumerable<string> raw)
    {
        var levels = new List<Level>();
        foreach (var value in raw.Where(v => !string.IsNullOrWhiteSpace(v)))
        {
            if (!Enum.TryParse<Level>(value.Trim(), ignoreCase: true, out var level))
                throw new RequestValidationException($"Unknown level '{value}'. Use Junior, Mid or Senior.");
            if (!levels.Contains(level)) levels.Add(level);
        }

        return levels;
    }

    /// <summary>Round-robins across technologies so a mixed session is not dominated by one of them.</summary>
    private static List<Question> SelectBalanced(IReadOnlyList<Question> candidates, int count, Random rng)
    {
        var queues = candidates
            .GroupBy(q => q.TechnologySlug)
            .OrderBy(g => g.Key)
            .Select(g => new Queue<Question>(g.OrderBy(_ => rng.Next())))
            .ToList();

        var picked = new List<Question>();
        while (picked.Count < count && queues.Any(q => q.Count > 0))
        {
            foreach (var queue in queues)
            {
                if (picked.Count >= count) break;
                if (queue.Count > 0) picked.Add(queue.Dequeue());
            }
        }

        return picked.OrderBy(_ => rng.Next()).ToList();
    }

    private static ScoreBreakdown Breakdown(string key, int total, int correct) =>
        new(key, total, correct, Percent(correct, total));

    private static int Percent(int correct, int total) =>
        total == 0 ? 0 : (int)Math.Round(correct * 100.0 / total);
}
