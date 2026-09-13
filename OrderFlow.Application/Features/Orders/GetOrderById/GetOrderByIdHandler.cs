using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;

namespace OrderFlow.Domain.Features.Orders.GetOrderById;

public class GetOrderByIdHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    public GetOrderByIdHandler(
        IApplicationDbContext context,
        ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"order:{request.Id}";
        var cachedOrder =
    await _cacheService.GetAsync<OrderDto>(
        cacheKey,
        cancellationToken);

        if (cachedOrder != null) {
            return cachedOrder;
        }

        var order =await _context.Orders
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new OrderDto(
                x.Id,
                x.CustomerId,
                x.Total,
                x.Status.ToString(),
                x.Items
                    .Select(item => new OrderItemDto(
                        item.Id,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice,
                        item.Total))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException(
                $"Order with id {request.Id} was not found.");
        }

        await _cacheService.SetAsync(
            cacheKey,
            order,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return order;
    }
}