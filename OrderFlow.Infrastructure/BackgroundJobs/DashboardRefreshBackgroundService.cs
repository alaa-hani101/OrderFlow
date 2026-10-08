using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderFlow.Application.common.Observability;
using Microsoft.Extensions.Logging;


namespace OrderFlow.Infrastructure.BackgroundJobs;

public class DashboardRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ILogger<DashboardRefreshBackgroundService> _logger;

    public DashboardRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DashboardRefreshBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                _logger.LogInformation("Starting dashboard refresh background job.");

                var refreshService =
                    scope.ServiceProvider
                        .GetRequiredService<IDashboardRefreshService>();

                await refreshService.RefreshAsync(stoppingToken);

                var pendingOrdersProcessor =
                    scope.ServiceProvider
                         .GetRequiredService<IPendingOrdersProcessor>();

                await pendingOrdersProcessor.ProcessAsync(stoppingToken);

                await refreshService.RefreshAsync(stoppingToken);

                // Increment the background worker runs counter

                OrderFlowMetrics.BackgroundWorkerRuns.Add(1);

                _logger.LogInformation("Dashboard refresh background job completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard refresh background job failed.");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(1),
                stoppingToken);
        }
    }
}