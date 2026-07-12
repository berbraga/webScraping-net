using WebScraping.Domain.Entities;

namespace WebScraping.Domain.Abstractions;

public interface ISearchRepository
{
    Task InsertAsync(Search search, CancellationToken cancellationToken = default);
    Task<Search?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Search search, CancellationToken cancellationToken = default);
}

public interface IBusinessRepository
{
    Task InsertManyAsync(IEnumerable<Business> businesses, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Business>> ListBySearchIdAsync(string searchId, int skip, int take, CancellationToken cancellationToken = default);
    Task<long> CountBySearchIdAsync(string searchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Business>> ListPendingBySearchIdAsync(string searchId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Business business, CancellationToken cancellationToken = default);
    Task MarkRemainingPendingAsSkippedAsync(string searchId, CancellationToken cancellationToken = default);
}

public sealed record BusinessListing(string Name, string? ExternalId);

public sealed record BusinessDetails(
    string? Phone,
    string? Website,
    double? Rating);

public interface IBusinessLookupSource
{
    Task<IReadOnlyList<BusinessListing>> SearchAsync(
        string region,
        string query,
        int maxResults,
        CancellationToken cancellationToken = default);

    Task<BusinessDetails> GetDetailsAsync(
        string externalId,
        CancellationToken cancellationToken = default);
}

public interface ISearchJobQueue
{
    ValueTask EnqueueAsync(string searchId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> DequeueAllAsync(CancellationToken cancellationToken);
}
