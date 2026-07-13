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
    private readonly ISearchJobQueue _queue;
    private readonly SearchOptions _options;

    public StartSearchHandler(
        ISearchRepository searches,
        ISearchJobQueue queue,
        IOptions<SearchOptions> options)
    {
        _searches = searches;
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
        await _queue.EnqueueAsync(search.Id, cancellationToken);
        return ToDto(search);
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
