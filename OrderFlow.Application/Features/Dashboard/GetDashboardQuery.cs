using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Dashboard
{
    public record GetDashboardQuery
    : IRequest<List<OrderDashboardDto>>;

    public class OrderDashboardDto
    {
        public int OrderId { get; set; }

        public string CustomerName { get; set; } = null!;

        public int ItemCount { get; set; }

        public decimal Total { get; set; }

        public string Status { get; set; } = null!;
    }
}
