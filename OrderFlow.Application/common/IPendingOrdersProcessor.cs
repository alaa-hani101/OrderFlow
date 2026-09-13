public interface IPendingOrdersProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}