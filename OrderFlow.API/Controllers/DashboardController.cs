using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Dashboard;

namespace OrderFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ISender _sender;

    public DashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetDashboardQuery(),
            cancellationToken);

        return Ok(result);
    }
}