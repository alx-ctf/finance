using FinTracker.Application.DTOs;
using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FluentValidation;

namespace FinTracker.Application.Services;

public class AccountService(
    IAccountRepository accountRepository,
    ITransactionRepository transactionRepository,
    IValidator<CreateAccountDto> validator) : IAccountService
{
    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
    {
        var accounts = await accountRepository.GetAllAsync(userId, cancellationToken);
        return accounts.Select(MapToDto).ToList();
    }

    public async Task<AccountDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(id, userId, cancellationToken);
        return account is null ? null : MapToDto(account);
    }

    public async Task<AccountDto> CreateAsync(string userId, CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var account = new Account
        {
            UserId = userId,
            Name = dto.Name,
            Type = dto.Type,
            Balance = dto.InitialBalance,
            Currency = dto.Currency,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await accountRepository.AddAsync(account, cancellationToken);
        return MapToDto(created);
    }

    public async Task<AccountDto> UpdateAsync(int id, string userId, CreateAccountDto dto, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var account = await accountRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Account not found.");

        account.Name = dto.Name;
        account.Type = dto.Type;
        account.Currency = dto.Currency;
        account.Description = dto.Description;

        await accountRepository.UpdateAsync(account, cancellationToken);
        return MapToDto(account);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(id, userId, cancellationToken)
            ?? throw new InvalidOperationException("Account not found.");

        if (await transactionRepository.HasTransactionsForAccountAsync(id, userId, cancellationToken))
        {
            throw new InvalidOperationException("Cannot delete an account that has transactions.");
        }

        await accountRepository.DeleteAsync(account, cancellationToken);
    }

    private static AccountDto MapToDto(Account account) => new()
    {
        Id = account.Id,
        Name = account.Name,
        Type = account.Type,
        Balance = account.Balance,
        Currency = account.Currency,
        IsActive = account.IsActive
    };
}
