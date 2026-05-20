using FinTracker.Application.DTOs;

namespace FinTracker.Application.Interfaces;

public interface IBudgetService
{
    Task<IReadOnlyList<BudgetDto>> GetAllAsync(
        string userId,
        int? year = null,
        int? month = null,
        CancellationToken cancellationToken = default);
    Task<BudgetDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<BudgetDto> CreateAsync(string userId, CreateBudgetDto dto, CancellationToken cancellationToken = default);
    Task<BudgetDto> UpdateAsync(int id, string userId, CreateBudgetDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
