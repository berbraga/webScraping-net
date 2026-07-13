using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;
using Microsoft.Extensions.Options;

namespace WebScraping.Infrastructure.Persistence;

internal sealed class SearchDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalFound { get; set; }
    public int ProcessedCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

internal sealed class BusinessDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;
    public string SearchId { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public int? SiteCreationYear { get; set; }
    public double? Rating { get; set; }
    public string DetailStatus { get; set; } = string.Empty;
    public string? DetailError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class MongoSearchRepository : ISearchRepository
{
    private readonly IMongoCollection<SearchDocument> _collection;

    public MongoSearchRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<SearchDocument>("searches");
    }

    public Task InsertAsync(Search search, CancellationToken cancellationToken = default) =>
        _collection.InsertOneAsync(ToDocument(search), cancellationToken: cancellationToken);

    public async Task<Search?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var doc = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        return doc is null ? null : FromDocument(doc);
    }

    public Task UpdateAsync(Search search, CancellationToken cancellationToken = default) =>
        _collection.ReplaceOneAsync(x => x.Id == search.Id, ToDocument(search), cancellationToken: cancellationToken);

    private static SearchDocument ToDocument(Search search) => new()
    {
        Id = search.Id,
        Region = search.Region,
        Query = search.Query,
        MaxResults = search.MaxResults,
        Status = search.Status.ToString(),
        TotalFound = search.TotalFound,
        ProcessedCount = search.ProcessedCount,
        FailedCount = search.FailedCount,
        ErrorMessage = search.ErrorMessage,
        CreatedAt = search.CreatedAt,
        UpdatedAt = search.UpdatedAt,
        CompletedAt = search.CompletedAt
    };

    private static Search FromDocument(SearchDocument doc) => new()
    {
        Id = doc.Id,
        Region = doc.Region,
        Query = doc.Query,
        MaxResults = doc.MaxResults,
        Status = Enum.Parse<SearchStatus>(doc.Status, true),
        TotalFound = doc.TotalFound,
        ProcessedCount = doc.ProcessedCount,
        FailedCount = doc.FailedCount,
        ErrorMessage = doc.ErrorMessage,
        CreatedAt = doc.CreatedAt,
        UpdatedAt = doc.UpdatedAt,
        CompletedAt = doc.CompletedAt
    };
}

public sealed class MongoBusinessRepository : IBusinessRepository
{
    private readonly IMongoCollection<BusinessDocument> _collection;

    public MongoBusinessRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<BusinessDocument>("businesses");
    }

    public Task InsertManyAsync(IEnumerable<Business> businesses, CancellationToken cancellationToken = default) =>
        _collection.InsertManyAsync(businesses.Select(ToDocument), cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Business>> ListBySearchIdAsync(string searchId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var docs = await _collection.Find(x => x.SearchId == searchId)
            .SortBy(x => x.CreatedAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync(cancellationToken);

        return docs.Select(FromDocument).ToList();
    }

    public Task<long> CountBySearchIdAsync(string searchId, CancellationToken cancellationToken = default) =>
        _collection.CountDocumentsAsync(x => x.SearchId == searchId, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Business>> ListPendingBySearchIdAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var status = DetailStatus.Pending.ToString();
        var docs = await _collection.Find(x => x.SearchId == searchId && x.DetailStatus == status)
            .SortBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return docs.Select(FromDocument).ToList();
    }

    public Task UpdateAsync(Business business, CancellationToken cancellationToken = default) =>
        _collection.ReplaceOneAsync(x => x.Id == business.Id, ToDocument(business), cancellationToken: cancellationToken);

    public async Task MarkRemainingPendingAsSkippedAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var pending = DetailStatus.Pending.ToString();
        var skipped = DetailStatus.Skipped.ToString();
        var update = Builders<BusinessDocument>.Update
            .Set(x => x.DetailStatus, skipped)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _collection.UpdateManyAsync(
            x => x.SearchId == searchId && x.DetailStatus == pending,
            update,
            cancellationToken: cancellationToken);
    }

    private static BusinessDocument ToDocument(Business business) => new()
    {
        Id = business.Id,
        SearchId = business.SearchId,
        ExternalId = business.ExternalId,
        Name = business.Name,
        Phone = business.Phone,
        Website = business.Website,
        SiteCreationYear = business.SiteCreationYear,
        Rating = business.Rating,
        DetailStatus = business.DetailStatus.ToString(),
        DetailError = business.DetailError,
        CreatedAt = business.CreatedAt,
        UpdatedAt = business.UpdatedAt
    };

    private static Business FromDocument(BusinessDocument doc) => new()
    {
        Id = doc.Id,
        SearchId = doc.SearchId,
        ExternalId = doc.ExternalId,
        Name = doc.Name,
        Phone = doc.Phone,
        Website = doc.Website,
        SiteCreationYear = doc.SiteCreationYear,
        Rating = doc.Rating,
        DetailStatus = Enum.Parse<DetailStatus>(doc.DetailStatus, true),
        DetailError = doc.DetailError,
        CreatedAt = doc.CreatedAt,
        UpdatedAt = doc.UpdatedAt
    };
}

public sealed class MongoIndexInitializer
{
    private readonly IMongoDatabase _database;

    public MongoIndexInitializer(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var businesses = _database.GetCollection<BusinessDocument>("businesses");

        var searchIdIndex = new CreateIndexModel<BusinessDocument>(
            Builders<BusinessDocument>.IndexKeys.Ascending(x => x.SearchId));

        var uniqueExternal = new CreateIndexModel<BusinessDocument>(
            Builders<BusinessDocument>.IndexKeys
                .Ascending(x => x.SearchId)
                .Ascending(x => x.ExternalId),
            new CreateIndexOptions<BusinessDocument>
            {
                Unique = true,
                PartialFilterExpression = Builders<BusinessDocument>.Filter.Type(x => x.ExternalId, BsonType.String)
            });

        await businesses.Indexes.CreateManyAsync(new[] { searchIdIndex, uniqueExternal }, cancellationToken);
    }
}
