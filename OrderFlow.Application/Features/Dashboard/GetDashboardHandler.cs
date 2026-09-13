using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;

namespace OrderFlow.Application.Features.Dashboard
{
    public class GetDashboardHandler
    : IRequestHandler<GetDashboardQuery, List<OrderDashboardDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDashboardHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDashboardDto>> Handle(
            GetDashboardQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.OrderDashboardReadModels
                .AsNoTracking()
                .Select(x => new OrderDashboardDto
                {
                    OrderId = x.OrderId,
                    CustomerName = x.CustomerName,
                    ItemCount = x.ItemCount,
                    Total = x.Total,
                    Status = x.Status
                })
                .ToListAsync(cancellationToken);
        }
    }
}
