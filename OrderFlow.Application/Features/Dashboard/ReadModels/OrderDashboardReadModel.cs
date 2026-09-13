namespace OrderFlow.Application.Features.Dashboard.ReadModels;

public class OrderDashboardReadModel
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerName { get; set; } = null!;

    public int ItemCount { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = null!;
}