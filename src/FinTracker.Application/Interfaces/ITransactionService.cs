using FinTracker.Application.DTOs;

namespace FinTracker.Application.Interfaces;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionDto>> GetAllAsync(
        string userId,
        int? accountId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);
    Task<TransactionDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<TransactionDto> CreateAsync(string userId, CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task<TransactionDto> UpdateAsync(int id, string userId, CreateTransactionDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
