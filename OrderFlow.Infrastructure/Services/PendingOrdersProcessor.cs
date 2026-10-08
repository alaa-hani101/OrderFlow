using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;
using OrderFlow.Application.common.Observability;
using OrderFlow.Domain.Entities.Enums;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Services;

public class PendingOrdersProcessor : IPendingOrdersProcessor
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public PendingOrdersProcessor(ApplicationDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        var pendingOrders = await _context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);

        OrderFlowMetrics.SetPendingOrders(pendingOrders.Count);

        foreach (var order in pendingOrders)
        {
            order.Status = OrderStatus.Completed;
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var order in pendingOrders)
        {
            await _cacheService.RemoveAsync(
                $"order:{order.Id}");
        }
    }
}