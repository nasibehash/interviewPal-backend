using InterviewPal.Domain;

namespace InterviewPal.Application.Abstractions;

public record QuestionFilter(
    string? Technology,
    Level? Level,
    string? Tag,
    string? Search,
    int Page,
    int PageSize);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public interface IQuestionRepository
{
    Task<IReadOnlyList<Technology>> GetTechnologiesAsync(CancellationToken ct);

    Task<Dictionary<(string Technology, Level Level), int>> CountByTechnologyAndLevelAsync(CancellationToken ct);

    Task<PagedResult<Question>> SearchAsync(QuestionFilter filter, CancellationToken ct);

    Task<Question?> GetAsync(string id, CancellationToken ct);

    Task<IReadOnlyList<Question>> GetByIdsAsync(IReadOnlyCollection<string> ids, CancellationToken ct);

    /// <summary>Questions matching the given technologies and levels. Empty collections mean "all".</summary>
    Task<IReadOnlyList<Question>> GetCandidatesAsync(
        IReadOnlyCollection<string> technologies,
        IReadOnlyCollection<Level> levels,
        CancellationToken ct);
}
