using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context) => _context = context;

    public async Task<Budget?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Budget>> GetAllAsync(
        string userId,
        int? year = null,
        int? month = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Budgets
            .AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId);

        if (year.HasValue)
            query = query.Where(b => b.Year == year.Value);

        if (month.HasValue)
            query = query.Where(b => b.Month == month.Value);

        return await query
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetByCategoryAndPeriodAsync(
        string userId,
        int categoryId,
        int year,
        int month,
        CancellationToken cancellationToken = default) =>
        await _context.Budgets
            .Include(b => b.Category)
            .FirstOrDefaultAsync(
                b => b.UserId == userId && b.CategoryId == categoryId && b.Year == year && b.Month == month,
                cancellationToken);

    public async Task<Budget> AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync(cancellationToken);
        return budget;
    }

    public async Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
