using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Domain.Entities;

public class OrderItem
{
    public int Id { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Total => Quantity * UnitPrice;

    public int OrderId { get; private set; }

    public Order Order { get; private set; } = null!;
    public OrderItem(
       string productName,
       int quantity,
       decimal unitPrice)
    {
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}