using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OrderFlow.Infrastructure.BackgroundJobs;

public class DashboardRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DashboardRefreshBackgroundService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var refreshService =
                    scope.ServiceProvider
                        .GetRequiredService<IDashboardRefreshService>();

                await refreshService.RefreshAsync(stoppingToken);

                var pendingOrdersProcessor =
                    scope.ServiceProvider
                         .GetRequiredService<IPendingOrdersProcessor>();

                await pendingOrdersProcessor.ProcessAsync(stoppingToken);

                await refreshService.RefreshAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // هنضيف Logging هنا بعدين
                Console.WriteLine(
                    $"Dashboard refresh failed: {ex.Message}");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}