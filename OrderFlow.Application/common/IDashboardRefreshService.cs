public interface IDashboardRefreshService
{
    Task RefreshAsync(CancellationToken cancellationToken);
}