using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Infrastructure.Lookup;

public sealed class GooglePlacesBusinessLookupSource : IBusinessLookupSource
{
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
        CancellationToken cancellationToken = default)
    {
        EnsureApiKey();

        var textQuery = $"{query} in {region}";
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchText");
        request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", _options.ApiKey);
        request.Headers.TryAddWithoutValidation("X-Goog-FieldMask", "places.id,places.displayName");
        request.Content = JsonContent.Create(new
        {
            textQuery,
            pageSize = Math.Clamp(maxResults, 1, 20)
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessOrThrowAsync(response, "Text Search", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = new List<BusinessListing>();
        if (!document.RootElement.TryGetProperty("places", out var places))
        {
            return results;
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

            results.Add(new BusinessListing(name, id));
            if (results.Count >= maxResults)
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
