using FinTracker.Application.DTOs;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Services;

public class DashboardService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var totalIncome = await transactionRepository.GetTotalByTypeAsync(
            userId, TransactionType.Income, from, to, cancellationToken);
        var totalExpense = await transactionRepository.GetTotalByTypeAsync(
            userId, TransactionType.Expense, from, to, cancellationToken);
        var balance = await accountRepository.GetTotalBalanceAsync(userId, cancellationToken);

        return new DashboardSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            Balance = balance
        };
    }

    public async Task<IReadOnlyList<CategoryExpenseDto>> GetExpensesByCategoryAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var aggregates = await transactionRepository.GetExpensesByCategoryAsync(
            userId, from, to, cancellationToken);

        return aggregates
            .Select(a => new CategoryExpenseDto
            {
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName,
                CategoryColor = a.CategoryColor,
                Amount = a.Amount
            })
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlyTrendDto>> GetMonthlyTrendAsync(
        string userId,
        int monthCount = 6,
        CancellationToken cancellationToken = default)
    {
        if (monthCount < 1)
        {
            monthCount = 1;
        }

        var aggregates = await transactionRepository.GetMonthlyTrendAsync(
            userId, monthCount, cancellationToken);

        return aggregates
            .Select(a => new MonthlyTrendDto
            {
                Year = a.Year,
                Month = a.Month,
                Income = a.Income,
                Expense = a.Expense
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();
    }

    public async Task<IReadOnlyList<DailyExpenseDto>> GetDailyExpensesAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var aggregates = await transactionRepository.GetDailyExpensesAsync(
            userId, from, to, cancellationToken);

        return aggregates
            .Select(a => new DailyExpenseDto
            {
                Day = a.Day,
                Label = a.Day.ToString(),
                Amount = a.Amount
            })
            .OrderBy(x => x.Day)
            .ToList();
    }

    public async Task<IReadOnlyList<AccountExpenseDto>> GetExpensesByAccountAsync(
        string userId,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var aggregates = await transactionRepository.GetExpensesByAccountAsync(
            userId, from, to, cancellationToken);

        return aggregates
            .Select(a => new AccountExpenseDto
            {
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                Amount = a.Amount
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }
}
