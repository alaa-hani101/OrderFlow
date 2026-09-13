using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;
using OrderFlow.Application.Features.Dashboard.ReadModels;
using OrderFlow.Infrastructure.Persistence;

namespace OrderFlow.Infrastructure.Services;

public class DashboardRefreshService : IDashboardRefreshService
{
    private readonly ApplicationDbContext _context;

    public DashboardRefreshService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RefreshAsync(
        CancellationToken cancellationToken)
    {
        var dashboardData = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .Select(o => new OrderDashboardReadModel
            {
                OrderId = o.Id,
                CustomerName = o.Customer.Name,
                ItemCount = o.Items.Count,
                Total = o.Items.Sum(x =>
                    x.Quantity * x.UnitPrice),
                Status = o.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Remove old read model
        _context.OrderDashboardReadModels.RemoveRange(
            _context.OrderDashboardReadModels);

        // Add the new read model
        await _context.OrderDashboardReadModels.AddRangeAsync(
            dashboardData,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}