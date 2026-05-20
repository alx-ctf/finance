using FinTracker.Application.DTOs;
using FinTracker.Domain.Enums;

namespace FinTracker.Application.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(string userId, TransactionType? type = null, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(string userId, CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(int id, string userId, CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
