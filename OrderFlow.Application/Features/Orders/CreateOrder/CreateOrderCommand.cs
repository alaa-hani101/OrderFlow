using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace OrderFlow.Application.Features.Orders.CreateOrder;

    public record CreateOrderCommand(
     int CustomerId,
     List<CreateOrderItemDto> Items
 ) : IRequest<int>;

    public record CreateOrderItemDto(
        string ProductName,
        int Quantity,
        decimal UnitPrice
   );

