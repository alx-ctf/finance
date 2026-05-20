using FinTracker.Application.Helpers;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context) => _context = context;

    public async Task<Transaction?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TransactionTags)
            .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(
        string userId,
        int? accountId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(userId, accountId, from, to);

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByTypeAsync(
        string userId,
        TransactionType type,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default) =>
        await BuildQuery(userId, null, from, to)
            .Where(t => t.Type == type)
            .SumAsync(t => t.Amount, cancellationToken);

    public async Task<decimal> GetExpenseSumForCategoryAsync(
        string userId,
        int categoryId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);

        return await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.CategoryId == categoryId &&
                t.Type == TransactionType.Expense &&
                t.Date >= start &&
                t.Date <= end)
            .SumAsync(t => t.Amount, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryExpenseAggregate>> GetExpensesByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense);

        var fromUtc = UtcDateHelper.ToUtcDate(from);
        var toUtc = UtcDateHelper.ToUtcDate(to);

        if (fromUtc.HasValue)
            query = query.Where(t => t.Date >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(t => t.Date <= toUtc.Value);

        return await query
            .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Color })
            .Select(g => new CategoryExpenseAggregate
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                CategoryColor = g.Key.Color,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyTrendAggregate>> GetMonthlyTrendAsync(
        string userId,
        int monthCount,
        CancellationToken cancellationToken = default)
    {
        if (monthCount < 1)
            monthCount = 1;

        var end = UtcDateHelper.ToUtcDate(DateTime.UtcNow);
        var start = new DateTime(end.Year, end.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(monthCount - 1));

        var rows = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= start)
            .GroupBy(t => new { t.Date.Year, t.Date.Month, t.Type })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Type,
                Total = g.Sum(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        var result = new List<MonthlyTrendAggregate>();
        for (var i = 0; i < monthCount; i++)
        {
            var point = start.AddMonths(i);
            var income = rows
                .Where(r => r.Year == point.Year && r.Month == point.Month && r.Type == TransactionType.Income)
                .Sum(r => r.Total);
            var expense = rows
                .Where(r => r.Year == point.Year && r.Month == point.Month && r.Type == TransactionType.Expense)
                .Sum(r => r.Total);

            result.Add(new MonthlyTrendAggregate
            {
                Year = point.Year,
                Month = point.Month,
                Income = income,
                Expense = expense
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<DailyExpenseAggregate>> GetDailyExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = UtcDateHelper.ToUtcDate(from);
        var toUtc = UtcDateHelper.ToUtcDate(to);

        if (!fromUtc.HasValue || !toUtc.HasValue)
            return [];

        var rows = await _context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.UserId == userId &&
                t.Type == TransactionType.Expense &&
                t.Date >= fromUtc.Value &&
                t.Date <= toUtc.Value)
            .GroupBy(t => t.Date.Day)
            .Select(g => new DailyExpenseAggregate
            {
                Day = g.Key,
                Amount = g.Sum(t => t.Amount)
            })
            .ToListAsync(cancellationToken);

        var daysInMonth = DateTime.DaysInMonth(fromUtc.Value.Year, fromUtc.Value.Month);
        var result = new List<DailyExpenseAggregate>(daysInMonth);
        for (var day = 1; day <= daysInMonth; day++)
        {
            result.Add(new DailyExpenseAggregate
            {
                Day = day,
                Amount = rows.FirstOrDefault(r => r.Day == day)?.Amount ?? 0
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<AccountExpenseAggregate>> GetExpensesByAccountAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense);

        var fromUtc = UtcDateHelper.ToUtcDate(from);
        var toUtc = UtcDateHelper.ToUtcDate(to);

        if (fromUtc.HasValue)
            query = query.Where(t => t.Date >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(t => t.Date <= toUtc.Value);

        return await query
            .GroupBy(t => new { t.AccountId, t.Account.Name })
            .Select(g => new AccountExpenseAggregate
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.Date = UtcDateHelper.ToUtcDate(transaction.Date);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        transaction.Date = UtcDateHelper.ToUtcDate(transaction.Date);
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetTagsAsync(
        int transactionId,
        IEnumerable<int> tagIds,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.TransactionTags
            .Where(tt => tt.TransactionId == transactionId)
            .ToListAsync(cancellationToken);

        _context.TransactionTags.RemoveRange(existing);

        var idList = tagIds.Distinct().ToList();
        foreach (var tagId in idList)
        {
            _context.TransactionTags.Add(new TransactionTag
            {
                TransactionId = transactionId,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasTransactionsForAccountAsync(
        int accountId,
        string userId,
        CancellationToken cancellationToken = default) =>
        await _context.Transactions.AnyAsync(
            t => t.AccountId == accountId && t.UserId == userId,
            cancellationToken);

    private IQueryable<Transaction> BuildQuery(
        string userId,
        int? accountId,
        DateTime? from,
        DateTime? to)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Include(t => t.TransactionTags)
            .ThenInclude(tt => tt.Tag)
            .Where(t => t.UserId == userId);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var fromUtc = UtcDateHelper.ToUtcDate(from);
        var toUtc = UtcDateHelper.ToUtcDate(to);

        if (fromUtc.HasValue)
            query = query.Where(t => t.Date >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(t => t.Date <= toUtc.Value);

        return query;
    }
}
