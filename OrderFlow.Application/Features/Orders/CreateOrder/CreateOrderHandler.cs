using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Domain.Entities;
using OrderFlow.Application.common;
using OrderFlow.Application.common.Observability;
using Microsoft.Extensions.Logging;

namespace OrderFlow.Application.Features.Orders.CreateOrder;

public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;

    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IApplicationDbContext context,
        ILogger<CreateOrderHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        using var activity = OrderFlowActivitySource.Source.StartActivity(
            "CreateOrderHandler.Handle");

        // 1. Check customer exists
        var customerExists = await _context.Customers
            .AnyAsync(
                x => x.Id == request.CustomerId,
                cancellationToken);

        if (!customerExists)
        {
            _logger.LogWarning(
    "Order creation failed. CustomerId: {CustomerId} was not found.",
    request.CustomerId);

            throw new KeyNotFoundException(
                $"Customer with id {request.CustomerId} was not found.");
        }

        // 2. Create Order
        var order = new Order(request.CustomerId);

        // 3. Add Order Items
        foreach (var item in request.Items)
        {
            order.AddItem(
                item.ProductName,
                item.Quantity,
                item.UnitPrice);
        }

        // 4. Add Order to DbContext
        _context.Orders.Add(order);

        // 5. Save changes
        await _context.SaveChangesAsync(
            cancellationToken);

        OrderFlowMetrics.OrdersCreated.Add(1);

        _logger.LogInformation(
    "Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}",
    order.Id,
    request.CustomerId);


        // 6. Return generated Order Id
        return order.Id;
    }
}