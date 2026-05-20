namespace FinTracker.Application.DTOs;

public class BudgetDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000";
    public decimal LimitAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Remaining => LimitAmount - SpentAmount;
    public double UsagePercent => LimitAmount > 0 ? (double)(SpentAmount / LimitAmount * 100) : 0;
}

public class CreateBudgetDto
{
    public int CategoryId { get; set; }
    public decimal LimitAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string? Note { get; set; }
}
