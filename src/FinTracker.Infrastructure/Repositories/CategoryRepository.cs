using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<Category?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllAsync(
        string userId,
        TransactionType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.AsNoTracking().Where(c => c.UserId == userId);
        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        return await query
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Categories.AnyAsync(c => c.Id == id && c.UserId == userId, cancellationToken);

    public async Task<bool> HasTransactionsAsync(int categoryId, string userId, CancellationToken cancellationToken = default) =>
        await _context.Transactions.AnyAsync(
            t => t.CategoryId == categoryId && t.UserId == userId,
            cancellationToken);
}
