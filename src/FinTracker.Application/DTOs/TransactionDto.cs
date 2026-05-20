using FinTracker.Domain.Enums;

namespace FinTracker.Application.DTOs;

public class TransactionDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#000";
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<int> TagIds { get; set; } = [];
}

public class CreateTransactionDto
{
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public List<int> TagIds { get; set; } = [];
}
