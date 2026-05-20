using FinTracker.Domain.Entities;

namespace FinTracker.Application.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> GetAllAsync(
        string userId,
        int? year = null,
        int? month = null,
        CancellationToken cancellationToken = default);
    Task<Budget?> GetByCategoryAndPeriodAsync(
        string userId,
        int categoryId,
        int year,
        int month,
        CancellationToken cancellationToken = default);
    Task<Budget> AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(Budget budget, CancellationToken cancellationToken = default);
}
