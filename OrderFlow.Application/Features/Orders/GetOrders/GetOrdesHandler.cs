using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.common;
using OrderFlow.Application.common.Observability;

namespace OrderFlow.Application.Features.Orders.GetOrders;

public class GetOrdersHandler
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    private readonly ILogger<GetOrdersHandler> _logger;

    public GetOrdersHandler(IApplicationDbContext context, ILogger<GetOrdersHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<OrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        using var activity = OrderFlowActivitySource.Source.StartActivity("GetOrdersHandler.Handle");

       var orders = await _context.Orders
            .AsNoTracking()
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.Total,
                order.Status.ToString()
            ))
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} orders from the database.", orders.Count);

        return orders;


    }
}