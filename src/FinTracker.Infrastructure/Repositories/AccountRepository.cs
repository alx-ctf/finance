using FinTracker.Application.Interfaces;
using FinTracker.Domain.Entities;
using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) => _context = context;

    public async Task<Account?> GetByIdAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Account>> GetAllAsync(string userId, CancellationToken cancellationToken = default) =>
        await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalBalanceAsync(string userId, CancellationToken cancellationToken = default) =>
        await _context.Accounts
            .Where(a => a.UserId == userId && a.IsActive)
            .SumAsync(a => a.Balance, cancellationToken);

    public async Task<Account> AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, string userId, CancellationToken cancellationToken = default) =>
        await _context.Accounts.AnyAsync(a => a.Id == id && a.UserId == userId, cancellationToken);
}
