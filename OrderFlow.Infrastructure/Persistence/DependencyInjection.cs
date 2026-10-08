using HealthChecks.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.common;
using OrderFlow.Infrastructure.BackgroundJobs;
using OrderFlow.Infrastructure.Caching;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.Infrastructure.Services;
using StackExchange.Redis;

namespace OrderFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(
        configuration.GetConnectionString("Redis")!));

        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped<IDashboardRefreshService, DashboardRefreshService>();

        services.AddScoped< IPendingOrdersProcessor,PendingOrdersProcessor>();

        services.AddHostedService<DashboardRefreshBackgroundService>();

        services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        "sql-server")
     .AddCheck<RedisHealthCheck>(
        "redis");

        return services;
    }
}