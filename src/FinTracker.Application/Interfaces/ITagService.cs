using FinTracker.Application.DTOs;

namespace FinTracker.Application.Interfaces;

public interface ITagService
{
    Task<IReadOnlyList<TagDto>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<TagDto?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<TagDto> CreateAsync(string userId, CreateTagDto dto, CancellationToken cancellationToken = default);
    Task<TagDto> UpdateAsync(int id, string userId, CreateTagDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, string userId, CancellationToken cancellationToken = default);
}
