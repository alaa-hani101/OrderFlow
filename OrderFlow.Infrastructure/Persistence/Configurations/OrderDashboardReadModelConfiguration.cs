using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Application.Features.Dashboard.ReadModels;

namespace OrderFlow.Infrastructure.Persistence.Configurations;

public class OrderDashboardReadModelConfiguration
    : IEntityTypeConfiguration<OrderDashboardReadModel>
{
    public void Configure(
        EntityTypeBuilder<OrderDashboardReadModel> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Total)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.OrderId)
            .IsUnique();
    }
}