using InterviewPal.Application.Abstractions;
using InterviewPal.Domain;
using Microsoft.EntityFrameworkCore;

namespace InterviewPal.Infrastructure.Persistence;

public class QuestionRepository(AppDbContext db) : IQuestionRepository
{
    public async Task<IReadOnlyList<Technology>> GetTechnologiesAsync(CancellationToken ct) =>
        await db.Technologies.AsNoTracking().OrderBy(t => t.Name).ToListAsync(ct);

    public async Task<Dictionary<(string Technology, Level Level), int>> CountByTechnologyAndLevelAsync(
        CancellationToken ct)
    {
        var rows = await db.Questions.AsNoTracking()
            .GroupBy(q => new { q.TechnologySlug, q.Level })
            .Select(g => new { g.Key.TechnologySlug, g.Key.Level, Count = g.Count() })
            .ToListAsync(ct);
        return rows.ToDictionary(r => (r.TechnologySlug, r.Level), r => r.Count);
    }

    public async Task<PagedResult<Question>> SearchAsync(QuestionFilter f, CancellationToken ct)
    {
        var query = db.Questions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Technology))
            query = query.Where(q => q.TechnologySlug == f.Technology);
        if (f.Level is { } level)
            query = query.Where(q => q.Level == level);
        if (!string.IsNullOrWhiteSpace(f.Tag))
            query = query.Where(q => q.Tags.Contains(f.Tag));
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var pattern = $"%{f.Search.Trim()}%";
            query = query.Where(q => EF.Functions.Like(q.Text, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .Include(q => q.Choices)
            .OrderBy(q => q.TechnologySlug).ThenBy(q => q.Level).ThenBy(q => q.Id)
            .Skip((f.Page - 1) * f.PageSize).Take(f.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Question>(items, f.Page, f.PageSize, total);
    }

    public Task<Question?> GetAsync(string id, CancellationToken ct) =>
        db.Questions.AsNoTracking().Include(q => q.Choices).FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task<IReadOnlyList<Question>> GetByIdsAsync(IReadOnlyCollection<string> ids, CancellationToken ct) =>
        await db.Questions.AsNoTracking().Include(q => q.Choices).Where(q => ids.Contains(q.Id)).ToListAsync(ct);

    public async Task<IReadOnlyList<Question>> GetCandidatesAsync(
        IReadOnlyCollection<string> technologies, IReadOnlyCollection<Level> levels, CancellationToken ct)
    {
        var query = db.Questions.AsNoTracking().Include(q => q.Choices).AsQueryable();
        if (technologies.Count > 0) query = query.Where(q => technologies.Contains(q.TechnologySlug));
        if (levels.Count > 0) query = query.Where(q => levels.Contains(q.Level));
        return await query.ToListAsync(ct);
    }
}
