using System.Diagnostics;

namespace OrderFlow.Application.common.Observability;

public static class OrderFlowActivitySource
{
    public const string Name = "OrderFlow";

    public static readonly ActivitySource Source =
        new(Name);
}