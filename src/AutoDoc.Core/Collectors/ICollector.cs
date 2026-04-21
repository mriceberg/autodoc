namespace AutoDoc.Core.Collectors;

public interface ICollector<T>
{
    Task<IReadOnlyList<T>> FetchAsync(CancellationToken ct = default);
}
