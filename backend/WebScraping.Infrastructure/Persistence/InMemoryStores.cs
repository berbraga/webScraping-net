using System.Collections.Concurrent;
using System.Threading.Channels;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Infrastructure.Persistence;

public sealed class InMemorySearchRepository : ISearchRepository
{
    private readonly ConcurrentDictionary<string, Search> _items = new();

    public Task InsertAsync(Search search, CancellationToken cancellationToken = default)
    {
        _items[search.Id] = Clone(search);
        return Task.CompletedTask;
    }

    public Task<Search?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.TryGetValue(id, out var search) ? Clone(search) : null);
    }

    public Task UpdateAsync(Search search, CancellationToken cancellationToken = default)
    {
        _items[search.Id] = Clone(search);
        return Task.CompletedTask;
    }

    private static Search Clone(Search search) => new()
    {
        Id = search.Id,
        Region = search.Region,
        Query = search.Query,
        MaxResults = search.MaxResults,
        Status = search.Status,
        TotalFound = search.TotalFound,
        ProcessedCount = search.ProcessedCount,
        FailedCount = search.FailedCount,
        ErrorMessage = search.ErrorMessage,
        CreatedAt = search.CreatedAt,
        UpdatedAt = search.UpdatedAt,
        CompletedAt = search.CompletedAt
    };
}

public sealed class InMemoryBusinessRepository : IBusinessRepository
{
    private readonly ConcurrentDictionary<string, Business> _items = new();

    public Task InsertManyAsync(IEnumerable<Business> businesses, CancellationToken cancellationToken = default)
    {
        foreach (var business in businesses)
        {
            _items[business.Id] = Clone(business);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Business>> ListBySearchIdAsync(string searchId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var items = _items.Values
            .Where(b => b.SearchId == searchId)
            .OrderBy(b => b.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(Clone)
            .ToList();

        return Task.FromResult<IReadOnlyList<Business>>(items);
    }

    public Task<long> CountBySearchIdAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var count = _items.Values.LongCount(b => b.SearchId == searchId);
        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<Business>> ListPendingBySearchIdAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var items = _items.Values
            .Where(b => b.SearchId == searchId && b.DetailStatus == DetailStatus.Pending)
            .OrderBy(b => b.CreatedAt)
            .Select(Clone)
            .ToList();

        return Task.FromResult<IReadOnlyList<Business>>(items);
    }

    public Task UpdateAsync(Business business, CancellationToken cancellationToken = default)
    {
        _items[business.Id] = Clone(business);
        return Task.CompletedTask;
    }

    public Task MarkRemainingPendingAsSkippedAsync(string searchId, CancellationToken cancellationToken = default)
    {
        foreach (var business in _items.Values.Where(b => b.SearchId == searchId && b.DetailStatus == DetailStatus.Pending))
        {
            business.DetailStatus = DetailStatus.Skipped;
            business.UpdatedAt = DateTime.UtcNow;
            _items[business.Id] = Clone(business);
        }

        return Task.CompletedTask;
    }

    private static Business Clone(Business business) => new()
    {
        Id = business.Id,
        SearchId = business.SearchId,
        ExternalId = business.ExternalId,
        Name = business.Name,
        Phone = business.Phone,
        Website = business.Website,
        SiteCreationYear = business.SiteCreationYear,
        Rating = business.Rating,
        DetailStatus = business.DetailStatus,
        DetailError = business.DetailError,
        CreatedAt = business.CreatedAt,
        UpdatedAt = business.UpdatedAt
    };
}

public sealed class InProcessSearchJobQueue : ISearchJobQueue
{
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public ValueTask EnqueueAsync(string searchId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(searchId, cancellationToken);

    public async IAsyncEnumerable<string> DequeueAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return item;
        }
    }
}
