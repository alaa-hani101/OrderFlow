using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;
using OrderFlow.Application.common.Observability;
using Microsoft.Extensions.Logging;


namespace OrderFlow.Application.Features.Orders.GetOrderById;

public class GetOrderByIdHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICacheService _cacheService;

    private readonly ILogger<GetOrderByIdHandler> _logger;

    public GetOrderByIdHandler(
        IApplicationDbContext context,
        ICacheService cacheService,
        ILogger<GetOrderByIdHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        using var activity = OrderFlowActivitySource.Source.StartActivity(
            "GetOrderByIdHandler.Handle");

        var cacheKey = $"order:{request.Id}";
        var cachedOrder =
    await _cacheService.GetAsync<OrderDto>(
        cacheKey,
        cancellationToken);

        if (cachedOrder != null) {
            _logger.LogInformation(
                "Order retrieved from cache. OrderId: {OrderId}",
                request.Id);
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
            _logger.LogWarning(
                "Order not found. OrderId: {OrderId}",
                request.Id);

            throw new KeyNotFoundException(
                $"Order with id {request.Id} was not found.");
        }

        _logger.LogInformation(
            "Order retrieved from database. OrderId: {OrderId}",
            request.Id);


        await _cacheService.SetAsync(
            cacheKey,
            order,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return order;
    }
}