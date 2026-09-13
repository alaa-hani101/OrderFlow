using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderFlow.Domain.Entities.Enums;

namespace OrderFlow.Domain.Entities;

public class Order
{
    public int Id { get; private set; }

    public int CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public decimal Total { get; private set; }

    public OrderStatus Status { get; set; }

    public ICollection<OrderItem> Items { get; private set; }
        = new List<OrderItem>();

    public Order(int customerId)
    {
        CustomerId = customerId;
        Status = OrderStatus.Pending;
    }

    public void AddItem(
        string productName,
        int quantity,
        decimal unitPrice)
    {
        var item = new OrderItem(
            productName,
            quantity,
            unitPrice);

        Items.Add(item);

        CalculateTotal();
    }

    private void CalculateTotal()
    {
        Total = Items.Sum(x => x.Total);
    }
}