using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;

namespace OrderFlow.Application.Features.Orders.GetOrders;

public class GetOrdersHandler
    : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Select(order => new OrderDto(
                order.Id,
                order.CustomerId,
                order.Total,
                order.Status.ToString()
            ))
            .ToListAsync(cancellationToken);
    }
}