using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Domain.Entities;

public class Customer
{
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ICollection<Order> Orders { get; private set; }
        = new List<Order>();
}