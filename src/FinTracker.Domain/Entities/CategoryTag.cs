namespace FinTracker.Domain.Entities;

/// <summary>
/// N:N между Category и Tag.
/// </summary>
public class CategoryTag
{
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
