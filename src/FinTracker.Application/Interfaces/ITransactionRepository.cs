using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetAllAsync(
        string userId,
        int? accountId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByTypeAsync(
        string userId,
        TransactionType type,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<decimal> GetExpenseSumForCategoryAsync(
        string userId,
        int categoryId,
        int year,
        int month,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryExpenseAggregate>> GetExpensesByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyTrendAggregate>> GetMonthlyTrendAsync(
        string userId,
        int monthCount,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyExpenseAggregate>> GetDailyExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountExpenseAggregate>> GetExpensesByAccountAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task SetTagsAsync(int transactionId, IEnumerable<int> tagIds, CancellationToken cancellationToken = default);
    Task<bool> HasTransactionsForAccountAsync(int accountId, string userId, CancellationToken cancellationToken = default);
}

public sealed class CategoryExpenseAggregate
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = "#000";
    public decimal Amount { get; init; }
}

public sealed class MonthlyTrendAggregate
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
}

public sealed class DailyExpenseAggregate
{
    public int Day { get; init; }
    public decimal Amount { get; init; }
}

public sealed class AccountExpenseAggregate
{
    public int AccountId { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}
