using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class GooglePlacesBusinessLookupSource : IBusinessLookupSource
{
    private const int GooglePageSizeLimit = 20;
    private const string TextSearchPath = "https://places.googleapis.com/v1/places:searchText";
    private const string FieldMask = "places.id,places.displayName,nextPageToken";

    private readonly HttpClient _httpClient;
    private readonly GooglePlacesOptions _options;

    public GooglePlacesBusinessLookupSource(HttpClient httpClient, IOptions<GooglePlacesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<BusinessListing>> SearchAsync(
        string region,
        string query,
        int maxResults,
        CancellationToken cancellationToken = default) =>
        await SearchAsync(region, query, maxResults, coverageSliceIndex: 0, cancellationToken);

    public async Task<IReadOnlyList<BusinessListing>> SearchAsync(
        string region,
        string query,
        int maxResults,
        int coverageSliceIndex,
        CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        if (maxResults <= 0)
        {
            return Array.Empty<BusinessListing>();
        }

        // coverageSliceIndex is used by Fake/orchestrator; Google relies on region/query variants from the planner.
        _ = coverageSliceIndex;

        var textQuery = $"{query} in {region}";
        var results = new List<BusinessListing>();
        var seenIds = new HashSet<string>(StringComparer.Ordinal);
        string? pageToken = null;

        while (results.Count < maxResults)
        {
            var pageSize = Math.Min(GooglePageSizeLimit, maxResults - results.Count);
            using var request = new HttpRequestMessage(HttpMethod.Post, TextSearchPath);
            request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", _options.ApiKey);
            request.Headers.TryAddWithoutValidation("X-Goog-FieldMask", FieldMask);

            object body = pageToken is null
                ? new { textQuery, pageSize }
                : new { textQuery, pageSize, pageToken };

            request.Content = JsonContent.Create(body);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessOrThrowAsync(response, "Text Search", cancellationToken);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            if (!document.RootElement.TryGetProperty("places", out var places)
                || places.ValueKind != JsonValueKind.Array
                || places.GetArrayLength() == 0)
            {
                break;
            }

            foreach (var place in places.EnumerateArray())
            {
                var id = place.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var name = place.TryGetProperty("displayName", out var display)
                    && display.TryGetProperty("text", out var text)
                    ? text.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var externalId = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
                if (externalId is not null && !seenIds.Add(externalId))
                {
                    continue;
                }

                results.Add(new BusinessListing(name.Trim(), externalId));
                if (results.Count >= maxResults)
                {
                    break;
                }
            }

            if (results.Count >= maxResults)
            {
                break;
            }

            if (!document.RootElement.TryGetProperty("nextPageToken", out var tokenProp))
            {
                break;
            }

            pageToken = tokenProp.GetString();
            if (string.IsNullOrWhiteSpace(pageToken))
            {
                break;
            }
        }

        return results;
    }

    public async Task<BusinessDetails> GetDetailsAsync(string externalId, CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var placeId = externalId.StartsWith("places/", StringComparison.Ordinal)
            ? externalId
            : $"places/{externalId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://places.googleapis.com/v1/{placeId}");
        request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation(
            "X-Goog-FieldMask",
            "nationalPhoneNumber,internationalPhoneNumber,websiteUri,rating");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, "Place Details", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        string? phone = null;
        if (root.TryGetProperty("nationalPhoneNumber", out var national))
        {
            phone = national.GetString();
        }
        else if (root.TryGetProperty("internationalPhoneNumber", out var international))
        {
            phone = international.GetString();
        }

        var website = root.TryGetProperty("websiteUri", out var websiteProp) ? websiteProp.GetString() : null;
        double? rating = root.TryGetProperty("rating", out var ratingProp) && ratingProp.TryGetDouble(out var value)
            ? value
            : null;

        return new BusinessDetails(phone, website, rating);
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Google Places API key is not configured.");
        }
    }

    private static async Task EnsureSuccessOrThrowAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Google Places {operation} failed with {(int)response.StatusCode} {response.ReasonPhrase}. {body}");
    }
}
