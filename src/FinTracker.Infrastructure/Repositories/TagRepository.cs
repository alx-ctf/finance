using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context) => _context = context;

    public async Task<Tag?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Tags.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetAllAsync(string userId, CancellationToken cancellationToken = default) =>
        await _context.Tags
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(
        IEnumerable<int> ids,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return [];

        return await _context.Tags
            .AsNoTracking()
            .Where(t => t.UserId == userId && idList.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AllExistAsync(
        IEnumerable<int> ids,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return true;

        var count = await _context.Tags.CountAsync(
            t => t.UserId == userId && idList.Contains(t.Id),
            cancellationToken);

        return count == idList.Count;
    }
}
