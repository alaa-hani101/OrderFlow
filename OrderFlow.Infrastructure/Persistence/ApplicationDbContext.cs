using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.common;
using OrderFlow.Application.Features.Dashboard.ReadModels;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Persistence
{
    public class ApplicationDbContext
    : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<OrderDashboardReadModel> OrderDashboardReadModels
            => Set<OrderDashboardReadModel>();
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
