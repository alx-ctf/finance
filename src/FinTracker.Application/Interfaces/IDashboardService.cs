using FinTracker.Application.DTOs;

namespace FinTracker.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryExpenseDto>> GetExpensesByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(
        string userId,
        int monthCount = 6,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DailyExpenseDto>> GetDailyExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountExpenseDto>> GetExpensesByAccountAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
}
