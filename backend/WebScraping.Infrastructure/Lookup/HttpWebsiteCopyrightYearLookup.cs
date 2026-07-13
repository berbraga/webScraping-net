using Microsoft.Extensions.Options;
using WebScraping.Application.Options;
using WebScraping.Domain.Abstractions;
using WebScraping.Domain.Services;

namespace WebScraping.Infrastructure.Lookup;

public sealed class HttpWebsiteCopyrightYearLookup : IWebsiteCopyrightYearLookup
{
    private readonly HttpClient _httpClient;
    private readonly WebsiteCopyrightOptions _options;

    public HttpWebsiteCopyrightYearLookup(HttpClient httpClient, IOptions<WebsiteCopyrightOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<int?> GetYearAsync(string websiteUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(websiteUrl))
        {
            return null;
        }

        try
        {
            if (!Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri)
                && !Uri.TryCreate($"https://{websiteUrl.Trim()}", UriKind.Absolute, out uri))
            {
                return null;
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));

            using var response = await _httpClient.GetAsync(uri, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var html = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            return CopyrightYearExtractor.TryExtractOldestYear(html);
        }
        catch
        {
            return null;
        }
    }
}
