using FinTracker.Application.DTOs;
using FinTracker.Application.Helpers;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository) : ITransactionService
{
    public async Task<IReadOnlyList<TransactionDto>> GetAllAsync(
        string userId,
        int? accountId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var transactions = await transactionRepository.GetAllAsync(
            userId, accountId, from, to, cancellationToken);

        return transactions.Select(MapToDto).ToList();
    }

    public async Task<TransactionDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, userId, cancellationToken);
        return transaction is null ? null : MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(
        string userId,
        CreateTransactionDto dto,
        CancellationToken cancellationToken = default)
    {
        await ValidateReferencesAsync(userId, dto, cancellationToken);

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = dto.AccountId,
            CategoryId = dto.CategoryId,
            Amount = dto.Amount,
            Type = dto.Type,
            Date = dto.Date,
            Note = dto.Note,
            CreatedAt = DateTime.UtcNow
        };

        var created = await transactionRepository.AddAsync(transaction, cancellationToken);
        await UpdateAccountBalanceAsync(
            dto.AccountId, userId, dto.Amount, dto.Type, apply: true, cancellationToken);

        if (dto.TagIds.Count > 0)
        {
            await transactionRepository.SetTagsAsync(created.Id, dto.TagIds, cancellationToken);
        }

        var loaded = await transactionRepository.GetByIdAsync(created.Id, userId, cancellationToken)
            ?? created;

        return MapToDto(loaded);
    }

    public async Task<TransactionDto> UpdateAsync(
        int id,
        string userId,
        CreateTransactionDto dto,
        CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Transaction not found.");

        await ValidateReferencesAsync(userId, dto, cancellationToken);

        var oldAccountId = transaction.AccountId;
        var oldAmount = transaction.Amount;
        var oldType = transaction.Type;

        transaction.AccountId = dto.AccountId;
        transaction.CategoryId = dto.CategoryId;
        transaction.Amount = dto.Amount;
        transaction.Type = dto.Type;
        transaction.Date = dto.Date;
        transaction.Note = dto.Note;

        if (oldAccountId == dto.AccountId)
        {
            var account = await accountRepository.GetByIdAsync(oldAccountId, userId, cancellationToken)
                ?? throw new InvalidOperationException("Account not found.");

            account.Balance = FinanceCalculator.ApplyBalanceChange(
                account.Balance, oldAmount, oldType, dto.Amount, dto.Type);
            await accountRepository.UpdateAsync(account, cancellationToken);
        }
        else
        {
            var oldAccount = await accountRepository.GetByIdAsync(oldAccountId, userId, cancellationToken)
                ?? throw new InvalidOperationException("Account not found.");
            oldAccount.Balance = FinanceCalculator.ReverseTransaction(
                oldAccount.Balance, oldAmount, oldType);
            await accountRepository.UpdateAsync(oldAccount, cancellationToken);

            var newAccount = await accountRepository.GetByIdAsync(dto.AccountId, userId, cancellationToken)
                ?? throw new InvalidOperationException("Account not found.");
            newAccount.Balance = FinanceCalculator.ApplyTransaction(
                newAccount.Balance, dto.Amount, dto.Type);
            await accountRepository.UpdateAsync(newAccount, cancellationToken);
        }

        await transactionRepository.UpdateAsync(transaction, cancellationToken);
        await transactionRepository.SetTagsAsync(transaction.Id, dto.TagIds, cancellationToken);

        var loaded = await transactionRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? transaction;

        return MapToDto(loaded);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Transaction not found.");

        await UpdateAccountBalanceAsync(
            transaction.AccountId,
            userId,
            transaction.Amount,
            transaction.Type,
            apply: false,
            cancellationToken);

        await transactionRepository.DeleteAsync(transaction, cancellationToken);
    }

    private async Task ValidateReferencesAsync(
        string userId,
        CreateTransactionDto dto,
        CancellationToken cancellationToken)
    {
        if (!await accountRepository.ExistsAsync(dto.AccountId, userId, cancellationToken))
        {
            throw new InvalidOperationException("Account not found.");
        }

        var category = await categoryRepository.GetByIdAsync(dto.CategoryId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Category not found.");

        if (category.Type != dto.Type)
        {
            throw new InvalidOperationException("Category type does not match transaction type.");
        }

        if (dto.TagIds.Count > 0 && !await tagRepository.AllExistAsync(dto.TagIds, userId, cancellationToken))
        {
            throw new InvalidOperationException("One or more tags were not found.");
        }
    }

    private async Task UpdateAccountBalanceAsync(
        int accountId,
        string userId,
        decimal amount,
        TransactionType type,
        bool apply,
        CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(accountId, userId, cancellationToken)
            ?? throw new InvalidOperationException("Account not found.");

        account.Balance = apply
            ? FinanceCalculator.ApplyTransaction(account.Balance, amount, type)
            : FinanceCalculator.ReverseTransaction(account.Balance, amount, type);

        await accountRepository.UpdateAsync(account, cancellationToken);
    }

    private static TransactionDto MapToDto(Transaction transaction) => new()
    {
        Id = transaction.Id,
        AccountId = transaction.AccountId,
        AccountName = transaction.Account?.Name ?? string.Empty,
        CategoryId = transaction.CategoryId,
        CategoryName = transaction.Category?.Name ?? string.Empty,
        CategoryColor = transaction.Category?.Color ?? "#000",
        Amount = transaction.Amount,
        Type = transaction.Type,
        Date = transaction.Date,
        Note = transaction.Note,
        Tags = transaction.TransactionTags?
            .Select(tt => tt.Tag?.Name ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList() ?? []
    };
}
