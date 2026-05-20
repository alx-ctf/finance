using FinTracker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.Infrastructure.Data;

public sealed class DatabaseTransaction(AppDbContext context) : IDatabaseTransaction
{
    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
