using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Features.Dashboard.ReadModels;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.common;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }

    DbSet<OrderItem> OrderItems { get; }

    DbSet<Customer> Customers { get; }

    DbSet<OrderDashboardReadModel> OrderDashboardReadModels { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}