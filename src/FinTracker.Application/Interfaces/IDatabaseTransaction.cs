namespace FinTracker.Application.Interfaces;

public interface IDatabaseTransaction
{
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
