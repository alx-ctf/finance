using FinTracker.Domain.Entities;

namespace FinTracker.Application.Interfaces;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<int> ids, string userId, CancellationToken cancellationToken = default);
    Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default);
    Task<bool> AllExistAsync(IEnumerable<int> ids, string userId, CancellationToken cancellationToken = default);
}
