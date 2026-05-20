namespace FinTracker.Domain.Entities;

public class Budget
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal LimitAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string? Note { get; set; }
}
