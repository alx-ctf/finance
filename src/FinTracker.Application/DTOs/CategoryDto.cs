using FinTracker.Domain.Enums;

namespace FinTracker.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Color { get; set; } = "#4CAF50";
    public string? Icon { get; set; }
}

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string Color { get; set; } = "#4CAF50";
    public string? Icon { get; set; }
}
