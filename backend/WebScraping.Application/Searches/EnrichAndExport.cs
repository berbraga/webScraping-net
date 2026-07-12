using System.Globalization;
using System.Text;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Entities;

namespace WebScraping.Application.Searches;

public sealed class EnrichBusinessesHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;
    private readonly IBusinessLookupSource _lookup;

    public EnrichBusinessesHandler(
        ISearchRepository searches,
        IBusinessRepository businesses,
        IBusinessLookupSource lookup)
    {
        _searches = searches;
        _businesses = businesses;
        _lookup = lookup;
    }

    public async Task EnrichSearchAsync(string searchId, CancellationToken cancellationToken = default)
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

        var pending = await _businesses.ListPendingBySearchIdAsync(searchId, cancellationToken);

        foreach (var business in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            search = await _searches.GetByIdAsync(searchId, cancellationToken);
            if (search is null || search.Status == SearchStatus.Cancelled)
            {
                await _businesses.MarkRemainingPendingAsSkippedAsync(searchId, cancellationToken);
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(business.ExternalId))
                {
                    business.DetailStatus = DetailStatus.Enriched;
                    business.Phone = null;
                    business.Website = null;
                    business.Rating = null;
                }
                else
                {
                    var details = await _lookup.GetDetailsAsync(business.ExternalId, cancellationToken);
                    business.Phone = details.Phone;
                    business.Website = details.Website;
                    business.Rating = details.Rating;
                    business.DetailStatus = DetailStatus.Enriched;
                }

                business.DetailError = null;
            }
            catch (Exception ex)
            {
                business.DetailStatus = DetailStatus.Failed;
                business.DetailError = ex.Message;
                search.FailedCount += 1;
            }

            business.UpdatedAt = DateTime.UtcNow;
            await _businesses.UpdateAsync(business, cancellationToken);

            search.ProcessedCount += 1;
            search.UpdatedAt = DateTime.UtcNow;
            await _searches.UpdateAsync(search, cancellationToken);
        }

        search = await _searches.GetByIdAsync(searchId, cancellationToken);
        if (search is null || search.Status == SearchStatus.Cancelled)
        {
            return;
        }

        search.Status = SearchStatus.Completed;
        search.UpdatedAt = DateTime.UtcNow;
        search.CompletedAt = search.UpdatedAt;
        await _searches.UpdateAsync(search, cancellationToken);
    }
}

public sealed class ExportSearchCsvHandler
{
    private readonly ISearchRepository _searches;
    private readonly IBusinessRepository _businesses;

    public ExportSearchCsvHandler(ISearchRepository searches, IBusinessRepository businesses)
    {
        _searches = searches;
        _businesses = businesses;
    }

    public async Task<(string FileName, byte[] Content)> HandleAsync(string searchId, CancellationToken cancellationToken = default)
    {
        var search = await _searches.GetByIdAsync(searchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Search '{searchId}' was not found.");

        if (search.Status == SearchStatus.Pending)
        {
            throw new InvalidOperationException("Search has no exportable results yet.");
        }

        var total = (int)await _businesses.CountBySearchIdAsync(searchId, cancellationToken);
        var items = await _businesses.ListBySearchIdAsync(searchId, 0, Math.Max(total, 1), cancellationToken);
        var csv = BuildCsv(items);
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        return ($"search-{searchId}.csv", bytes);
    }

    public static string BuildCsv(IEnumerable<Business> businesses)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Nome,Telefone,Site,Avaliacao");

        foreach (var business in businesses)
        {
            builder.Append(Escape(business.Name));
            builder.Append(',');
            builder.Append(Escape(business.Phone));
            builder.Append(',');
            builder.Append(Escape(business.Website));
            builder.Append(',');
            builder.Append(Escape(business.Rating?.ToString(CultureInfo.InvariantCulture)));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        var escaped = value.Replace("\"", "\"\"");
        return needsQuotes ? $"\"{escaped}\"" : escaped;
    }
}
