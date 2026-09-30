using InterviewPal.Application.Abstractions;
using InterviewPal.Application.Contracts;
using InterviewPal.Domain;

namespace InterviewPal.Application.Services;

public class QuestionService(IQuestionRepository questions)
{
    public async Task<IReadOnlyList<TechnologyDto>> GetTechnologiesAsync(CancellationToken ct)
    {
        var technologies = await questions.GetTechnologiesAsync(ct);
        var counts = await questions.CountByTechnologyAndLevelAsync(ct);

        return technologies
            .Select(t =>
            {
                var byLevel = Enum.GetValues<Level>()
                    .ToDictionary(l => l.ToString(), l => counts.GetValueOrDefault((t.Slug, l)));
                return new TechnologyDto(t.Slug, t.Name, t.CurrentVersion, t.SupportedFrom, byLevel.Values.Sum(), byLevel);
            })
            .ToList();
    }

    public async Task<PagedResult<QuestionDto>> SearchAsync(
        string? technology, string? level, string? tag, string? search, int page, int pageSize, CancellationToken ct)
    {
        Level? parsedLevel = null;
        if (!string.IsNullOrWhiteSpace(level))
        {
            if (!Enum.TryParse<Level>(level, ignoreCase: true, out var l))
                throw new RequestValidationException($"Unknown level '{level}'. Use Junior, Mid or Senior.");
            parsedLevel = l;
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await questions.SearchAsync(
            new QuestionFilter(technology?.ToLowerInvariant(), parsedLevel, tag, search, page, pageSize), ct);

        return new PagedResult<QuestionDto>(
            result.Items.Select(QuestionMapper.ToDto).ToList(), result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<QuestionDetailDto> GetAsync(string id, CancellationToken ct)
    {
        var q = await questions.GetAsync(id, ct)
                ?? throw new NotFoundException($"Question '{id}' was not found.");
        return new QuestionDetailDto(QuestionMapper.ToDto(q), QuestionMapper.ToAnswer(q));
    }
}
