using FinTracker.Domain.Enums;

namespace FinTracker.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Color { get; set; } = "#4CAF50";
    public string? Icon { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
    public ICollection<CategoryTag> CategoryTags { get; set; } = [];
}
