namespace FinTracker.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#9E9E9E";

    public ICollection<TransactionTag> TransactionTags { get; set; } = [];
    public ICollection<CategoryTag> CategoryTags { get; set; } = [];
}
