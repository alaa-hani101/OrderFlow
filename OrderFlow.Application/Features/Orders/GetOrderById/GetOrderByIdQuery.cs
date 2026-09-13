using MediatR;

namespace OrderFlow.Domain.Features.Orders.GetOrderById;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;

public record OrderDto(
    int Id,
    int CustomerId,
    decimal Total,
    string Status,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    int Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Total
);