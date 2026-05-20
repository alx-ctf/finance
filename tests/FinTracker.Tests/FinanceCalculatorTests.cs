using FinTracker.Application.Helpers;
using FinTracker.Domain.Enums;
using FluentAssertions;

namespace FinTracker.Tests;

public class FinanceCalculatorTests
{
    [Theory]
    [InlineData(TransactionType.Income, 100, 100)]
    [InlineData(TransactionType.Expense, 50, -50)]
    public void GetSignedAmount_ReturnsCorrectSign(TransactionType type, decimal amount, decimal expected)
    {
        FinanceCalculator.GetSignedAmount(amount, type).Should().Be(expected);
    }

    [Fact]
    public void ApplyTransaction_Income_IncreasesBalance()
    {
        FinanceCalculator.ApplyTransaction(1000m, 200m, TransactionType.Income).Should().Be(1200m);
    }

    [Fact]
    public void ApplyTransaction_Expense_DecreasesBalance()
    {
        FinanceCalculator.ApplyTransaction(1000m, 150m, TransactionType.Expense).Should().Be(850m);
    }

    [Fact]
    public void ReverseTransaction_RestoresPreviousBalance()
    {
        FinanceCalculator.ReverseTransaction(850m, 150m, TransactionType.Expense).Should().Be(1000m);
    }

    [Fact]
    public void ApplyBalanceChange_FromExpenseToIncome_AdjustsCorrectly()
    {
        var updated = FinanceCalculator.ApplyBalanceChange(
            900m, 100m, TransactionType.Expense, 100m, TransactionType.Income);
        updated.Should().Be(1100m);
    }
}
