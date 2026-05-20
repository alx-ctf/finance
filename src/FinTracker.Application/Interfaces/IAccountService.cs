using FinTracker.Application.DTOs;

namespace FinTracker.Application.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<AccountDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<AccountDto> CreateAsync(string userId, CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task<AccountDto> UpdateAsync(int id, string userId, CreateAccountDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
