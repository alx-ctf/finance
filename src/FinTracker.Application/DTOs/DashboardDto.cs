namespace FinTracker.Application.DTOs;

public class DashboardSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public decimal NetIncome => TotalIncome - TotalExpense;
}

public class CategoryExpenseDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000";
    public decimal Amount { get; set; }
}

public class MonthlyTrendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label => $"{Month:00}.{Year}";
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Net => Income - Expense;
}

public class DailyExpenseDto
{
    public int Day { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class AccountExpenseDto
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
