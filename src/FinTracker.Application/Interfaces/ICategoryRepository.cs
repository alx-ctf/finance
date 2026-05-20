using FinTracker.Domain.Entities;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(string userId, TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<bool> HasTransactionsAsync(int categoryId, string userId, CancellationToken cancellationToken = default);
}
