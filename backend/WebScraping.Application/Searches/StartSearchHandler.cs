using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Application.Searches;

public sealed record StartSearchRequest(string Region, string Query, int? MaxResults);

public sealed record SearchSummaryDto(
    string Id,
    string Region,
    string Query,
    int MaxResults,
    string Status,
    int TotalFound,
    int ProcessedCount,
    int FailedCount,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CompletedAt);

public sealed class StartSearchHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;
    private readonly IBusinessLookupSource _lookup;
    private readonly ISearchJobQueue _queue;
    private readonly SearchOptions _options;

    public StartSearchHandler(
        ISearchRepository searches,
        IBusinessRepository businesses,
        IBusinessLookupSource lookup,
        ISearchJobQueue queue,
        IOptions<SearchOptions> options)
    {
        _searches = searches;
        _businesses = businesses;
        _lookup = lookup;
        _queue = queue;
        _options = options.Value;
    }

    public async Task<SearchSummaryDto> HandleAsync(StartSearchRequest request, CancellationToken cancellationToken = default)
    {
        var maxResults = request.MaxResults ?? _options.DefaultMaxResults;
        Search.EnsureMaxResults(maxResults, _options.AbsoluteMaxResults);

        var now = DateTime.UtcNow;
        var search = Search.Create(request.Region, request.Query, maxResults, now);
        search.Status = SearchStatus.Running;
        search.UpdatedAt = now;

        await _searches.InsertAsync(search, cancellationToken);

        try
        {
            var listings = await _lookup.SearchAsync(search.Region, search.Query, search.MaxResults, cancellationToken);
            var distinct = Deduplicate(listings);

            var businesses = distinct
                .Select(item => Business.CreateDiscovered(search.Id, item.Name, item.ExternalId, DateTime.UtcNow))
                .ToList();

            if (businesses.Count > 0)
            {
                await _businesses.InsertManyAsync(businesses, cancellationToken);
            }

            search.TotalFound = businesses.Count;
            search.UpdatedAt = DateTime.UtcNow;

            if (businesses.Count == 0)
            {
                search.Status = SearchStatus.Completed;
                search.CompletedAt = search.UpdatedAt;
                await _searches.UpdateAsync(search, cancellationToken);
                return ToDto(search);
            }

            await _searches.UpdateAsync(search, cancellationToken);
            await _queue.EnqueueAsync(search.Id, cancellationToken);
            return ToDto(search);
        }
        catch (Exception ex)
        {
            search.Status = SearchStatus.Failed;
            search.ErrorMessage = ex.Message;
            search.UpdatedAt = DateTime.UtcNow;
            search.CompletedAt = search.UpdatedAt;
            await _searches.UpdateAsync(search, cancellationToken);
            throw;
        }
    }

    private static IReadOnlyList<BusinessListing> Deduplicate(IReadOnlyList<BusinessListing> listings)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<BusinessListing>();

        foreach (var item in listings)
        {
            var key = !string.IsNullOrWhiteSpace(item.ExternalId)
                ? $"id:{item.ExternalId}"
                : $"name:{item.Name}";

            if (seen.Add(key))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public static SearchSummaryDto ToDto(Search search) =>
        new(
            search.Id,
            search.Region,
            search.Query,
            search.MaxResults,
            search.Status.ToString().ToLowerInvariant(),
            search.TotalFound,
            search.ProcessedCount,
            search.FailedCount,
            search.ErrorMessage,
            search.CreatedAt,
            search.UpdatedAt,
            search.CompletedAt);
}
