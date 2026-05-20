using FinTracker.Domain.Entities;

namespace FinTracker.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalBalanceAsync(string userId, CancellationToken cancellationToken = default);
    Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task DeleteAsync(Account account, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, string userId, CancellationToken cancellationToken = default);
}
