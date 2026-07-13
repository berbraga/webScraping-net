using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Application.Searches;

public sealed class EnrichSiteCreationYearsHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;
    private readonly IWebsiteCopyrightYearLookup _lookup;
    private readonly WebsiteCopyrightOptions _options;

    public EnrichSiteCreationYearsHandler(
        ISearchRepository searches,
        IBusinessRepository businesses,
        IWebsiteCopyrightYearLookup lookup,
        IOptions<WebsiteCopyrightOptions> options)
    {
        _searches = searches;
        _businesses = businesses;
        _lookup = lookup;
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

        var total = (int)await _businesses.CountBySearchIdAsync(searchId, cancellationToken);
        if (total == 0)
        {
            return;
        }

        var businesses = await _businesses.ListBySearchIdAsync(searchId, 0, Math.Max(total, 1), cancellationToken);
        var byUrl = new Dictionary<string, List<Business>>(StringComparer.OrdinalIgnoreCase);

        foreach (var business in businesses)
        {
            if (string.IsNullOrWhiteSpace(business.Website))
            {
                business.SiteCreationYear = null;
                business.UpdatedAt = DateTime.UtcNow;
                await _businesses.UpdateAsync(business, cancellationToken);
                continue;
            }

            var key = NormalizeUrl(business.Website);
            if (!byUrl.TryGetValue(key, out var list))
            {
                list = [];
                byUrl[key] = list;
            }

            list.Add(business);
        }

        if (byUrl.Count == 0)
        {
            return;
        }

        var degree = Math.Max(1, _options.MaxDegreeOfParallelism);
        using var gate = new SemaphoreSlim(degree, degree);
        var results = new System.Collections.Concurrent.ConcurrentDictionary<string, int?>(
            StringComparer.OrdinalIgnoreCase);

        var tasks = byUrl.Keys.Select(async urlKey =>
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                search = await _searches.GetByIdAsync(searchId, cancellationToken);
                if (search is null || search.Status is SearchStatus.Cancelled or SearchStatus.Failed)
                {
                    results[urlKey] = null;
                    return;
                }

                var sampleUrl = byUrl[urlKey][0].Website!;
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
                    results[urlKey] = await _lookup.GetYearAsync(sampleUrl, timeoutCts.Token);
                }
                catch
                {
                    results[urlKey] = null;
                }
            }
            finally
            {
                gate.Release();
            }
        });

        await Task.WhenAll(tasks);

        foreach (var (urlKey, items) in byUrl)
        {
            results.TryGetValue(urlKey, out var year);
            foreach (var business in items)
            {
                business.SiteCreationYear = year;
                business.UpdatedAt = DateTime.UtcNow;
                await _businesses.UpdateAsync(business, cancellationToken);
            }
        }
    }

    internal static string NormalizeUrl(string url) => url.Trim().TrimEnd('/').ToLowerInvariant();
}
