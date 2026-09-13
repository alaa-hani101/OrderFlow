namespace OrderFlow.Application.Features.Orders.GetOrders;

using MediatR;


public record GetOrdersQuery : IRequest<List<OrderDto>>;
public record OrderDto(
    int Id,
    int CustomerId,
    decimal Total,
    string Status
);
