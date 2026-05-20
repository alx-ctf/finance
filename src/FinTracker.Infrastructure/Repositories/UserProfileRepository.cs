using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(AppDbContext context) : base(context) { }

    public async Task<UserProfile?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
}
