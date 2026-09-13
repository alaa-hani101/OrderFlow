using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Features.Orders.CreateOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Application.common;

namespace OrderFlow.Domain.Features.Orders.CreateOrder;

public class CreateOrderHandler
    : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Check customer exists
        var customerExists = await _context.Customers
            .AnyAsync(
                x => x.Id == request.CustomerId,
                cancellationToken);

        if (!customerExists)
        {
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

        // 6. Return generated Order Id
        return order.Id;
    }
}