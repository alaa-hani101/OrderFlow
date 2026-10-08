using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderFlow.Application.common.Observability;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _redis;

    public RedisHealthCheck(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _redis.GetDatabase();

            await database.PingAsync();

            OrderFlowMetrics.SetRedisHealth(true);

            return HealthCheckResult.Healthy(
                "Redis is available.");
        }
        catch (Exception ex)
        {
            OrderFlowMetrics.SetRedisHealth(false);

            return HealthCheckResult.Unhealthy(
                "Redis is unavailable.",
                ex);
        }
    }
}