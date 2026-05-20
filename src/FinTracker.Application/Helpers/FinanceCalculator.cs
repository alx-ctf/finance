using FinTracker.Domain.Enums;

namespace FinTracker.Application.Helpers;

public static class FinanceCalculator
{
    public static decimal GetSignedAmount(decimal amount, TransactionType type) =>
        type == TransactionType.Income ? amount : -amount;

    public static decimal ApplyTransaction(decimal currentBalance, decimal amount, TransactionType type) =>
        currentBalance + GetSignedAmount(amount, type);

    public static decimal ReverseTransaction(decimal currentBalance, decimal amount, TransactionType type) =>
        currentBalance - GetSignedAmount(amount, type);

    public static decimal ApplyBalanceChange(
        decimal currentBalance,
        decimal oldAmount,
        TransactionType oldType,
        decimal newAmount,
        TransactionType newType) =>
        ApplyTransaction(
            ReverseTransaction(currentBalance, oldAmount, oldType),
            newAmount,
            newType);
}
