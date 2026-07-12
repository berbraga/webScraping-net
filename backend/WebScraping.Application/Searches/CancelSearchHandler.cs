using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Application.Searches;

public sealed class CancelSearchHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;

    public CancelSearchHandler(ISearchRepository searches, IBusinessRepository businesses)
    {
        _searches = searches;
        _businesses = businesses;
    }

    public async Task<SearchSummaryDto> HandleAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var search = await _searches.GetByIdAsync(searchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Search '{searchId}' was not found.");

        if (search.Status is SearchStatus.Completed or SearchStatus.Failed or SearchStatus.Cancelled)
        {
            throw new InvalidOperationException($"Search '{searchId}' is already terminal ({search.Status}).");
        }

        search.Status = SearchStatus.Cancelled;
        search.UpdatedAt = DateTime.UtcNow;
        search.CompletedAt = search.UpdatedAt;
        await _searches.UpdateAsync(search, cancellationToken);
        await _businesses.MarkRemainingPendingAsSkippedAsync(searchId, cancellationToken);

        return StartSearchHandler.ToDto(search);
    }
}
