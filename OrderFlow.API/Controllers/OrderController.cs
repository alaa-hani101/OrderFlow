using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Orders.CreateOrder;
using OrderFlow.Application.Features.Orders.GetOrders;
using OrderFlow.Domain.Features.Orders.CreateOrder;
using OrderFlow.Domain.Features.Orders.GetOrderById;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var orderId = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(CreateOrder),
            new { id = orderId },
            new
            {
                id = orderId,
                message = "Order created successfully."
            });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
    int id,
    CancellationToken cancellationToken)
    {
        var order = await _sender.Send(
            new GetOrderByIdQuery(id),
            cancellationToken);

        if (order is null)
            return NotFound();

        return Ok(order);
    }
    [HttpGet]
    public async Task<IActionResult> GetOrders(
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetOrdersQuery(),
            cancellationToken);

        return Ok(result);
    }

}

