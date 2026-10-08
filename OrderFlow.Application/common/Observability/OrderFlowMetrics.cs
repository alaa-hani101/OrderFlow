using System.Diagnostics.Metrics;

namespace OrderFlow.Application.common.Observability;

public static class OrderFlowMetrics
{
    #region Orders Created Counter
    public static readonly Meter Meter =
        new("OrderFlow", "1.0.0");

    public static readonly Counter<long> OrdersCreated =
        Meter.CreateCounter<long>(
            "orderflow_orders_created_total");
    #endregion

    #region Pending Orders Gauge


    private static int _pendingOrders;

    public static readonly ObservableGauge<int> PendingOrders =
        Meter.CreateObservableGauge(
            "orderflow_pending_orders",
            () => _pendingOrders);

    public static void SetPendingOrders(int count)
    {
        _pendingOrders = count;
    }
    #endregion


    #region Background Worker Runs Counter
    public static readonly Counter<long> BackgroundWorkerRuns =
    Meter.CreateCounter<long>(
        "orderflow_background_worker_runs_total");

    #endregion

    #region Redis Health Check

    private static int _redisHealth = 1;

    public static readonly ObservableGauge<int> RedisHealth =
        Meter.CreateObservableGauge(
            "orderflow_redis_health",
            () => _redisHealth);

    public static void SetRedisHealth(bool isHealthy)
    {
        _redisHealth = isHealthy ? 1 : 0;
    }

    #endregion
}
