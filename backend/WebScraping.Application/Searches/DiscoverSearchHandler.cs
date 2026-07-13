using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Application.Searches;

public sealed class DiscoverSearchHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;
    private readonly IBusinessLookupSource _lookup;
    private readonly ITextCoveragePlanner _planner;
    private readonly EnrichBusinessesHandler _enricher;
    private readonly SearchOptions _options;

    public DiscoverSearchHandler(
        ISearchRepository searches,
        IBusinessRepository businesses,
        IBusinessLookupSource lookup,
        ITextCoveragePlanner planner,
        EnrichBusinessesHandler enricher,
        IOptions<SearchOptions> options)
    {
        _searches = searches;
        _businesses = businesses;
        _lookup = lookup;
        _planner = planner;
        _enricher = enricher;
        _options = options.Value;
    }

    public async Task HandleAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var search = await _searches.GetByIdAsync(searchId, cancellationToken);
        if (search is null)
        {
            return;
        }

        if (search.Status is SearchStatus.Cancelled or SearchStatus.Failed or SearchStatus.Completed)
        {
            return;
        }

        var pageCap = Math.Max(1, _options.ProviderPageCap);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var slices = _planner.Build(search.Region, search.Query);

        try
        {
            foreach (var slice in slices)
            {
                cancellationToken.ThrowIfCancellationRequested();

                search = await _searches.GetByIdAsync(searchId, cancellationToken);
                if (search is null || search.Status == SearchStatus.Cancelled)
                {
                    return;
                }

                if (search.Status is SearchStatus.Failed or SearchStatus.Completed)
                {
                    return;
                }

                var remaining = search.MaxResults - search.TotalFound;
                if (remaining <= 0)
                {
                    break;
                }

                var requestCount = Math.Min(remaining, pageCap);
                IReadOnlyList<BusinessListing> listings;
                try
                {
                    listings = await _lookup.SearchAsync(
                        slice.EffectiveRegion,
                        slice.EffectiveQuery,
                        requestCount,
                        slice.Index,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    await MarkFailedAsync(search, ex.Message, cancellationToken);
                    return;
                }

                var distinctNew = TakeNewDistinct(listings, seen, remaining);
                if (distinctNew.Count == 0)
                {
                    if (search.TotalFound == 0 && slice.Index == 0)
                    {
                        search.Status = SearchStatus.Completed;
                        search.UpdatedAt = DateTime.UtcNow;
                        search.CompletedAt = search.UpdatedAt;
                        await _searches.UpdateAsync(search, cancellationToken);
                        return;
                    }

                    break;
                }

                var businesses = distinctNew
                    .Select(item => Business.CreateDiscovered(search.Id, item.Name, item.ExternalId, DateTime.UtcNow))
                    .ToList();

                await _businesses.InsertManyAsync(businesses, cancellationToken);

                search.TotalFound += businesses.Count;
                search.UpdatedAt = DateTime.UtcNow;
                await _searches.UpdateAsync(search, cancellationToken);

                await _enricher.EnrichPendingAsync(searchId, cancellationToken);

                search = await _searches.GetByIdAsync(searchId, cancellationToken);
                if (search is null || search.Status == SearchStatus.Cancelled)
                {
                    return;
                }

                if (search.Status is SearchStatus.Failed)
                {
                    return;
                }

                if (search.TotalFound >= search.MaxResults)
                {
                    break;
                }
            }

            search = await _searches.GetByIdAsync(searchId, cancellationToken);
            if (search is null || search.Status is SearchStatus.Cancelled or SearchStatus.Failed)
            {
                return;
            }

            await _enricher.EnrichPendingAsync(searchId, cancellationToken);

            search = await _searches.GetByIdAsync(searchId, cancellationToken);
            if (search is null || search.Status is SearchStatus.Cancelled or SearchStatus.Failed)
            {
                return;
            }

            search.Status = SearchStatus.Completed;
            search.UpdatedAt = DateTime.UtcNow;
            search.CompletedAt = search.UpdatedAt;
            await _searches.UpdateAsync(search, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            search = await _searches.GetByIdAsync(searchId, cancellationToken);
            if (search is null || search.Status is SearchStatus.Cancelled or SearchStatus.Failed or SearchStatus.Completed)
            {
                return;
            }

            await MarkFailedAsync(search, ex.Message, cancellationToken);
        }
    }

    private async Task MarkFailedAsync(Search search, string message, CancellationToken cancellationToken)
    {
        search.Status = SearchStatus.Failed;
        search.ErrorMessage = message;
        search.UpdatedAt = DateTime.UtcNow;
        search.CompletedAt = search.UpdatedAt;
        await _searches.UpdateAsync(search, cancellationToken);
    }

    private static List<BusinessListing> TakeNewDistinct(
        IReadOnlyList<BusinessListing> listings,
        HashSet<string> seen,
        int remaining)
    {
        var result = new List<BusinessListing>();
        foreach (var item in listings)
        {
            if (result.Count >= remaining)
            {
                break;
            }

            var key = !string.IsNullOrWhiteSpace(item.ExternalId)
                ? $"id:{item.ExternalId}"
                : $"name:{item.Name}";

            if (!seen.Add(key))
            {
                continue;
            }

            result.Add(item);
        }

        return result;
    }
}
