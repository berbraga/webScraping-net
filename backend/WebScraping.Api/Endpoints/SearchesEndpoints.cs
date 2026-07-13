using WebScraping.Application.Searches;
using WebScraping.Domain.Abstractions;

namespace WebScraping.Api.Endpoints;

public static class SearchesEndpoints
{
    public static IEndpointRouteBuilder MapSearchesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/searches");

        group.MapPost("/", async (StartSearchRequest request, StartSearchHandler handler, CancellationToken ct) =>
        {
            try
            {
                var summary = await handler.HandleAsync(request, ct);
                return Results.Created($"/api/searches/{summary.Id}", summary);
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(title: "Validation failed", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(
                    title: "Lookup provider error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status502BadGateway);
            }
            catch (HttpRequestException ex)
            {
                return Results.Problem(
                    title: "Lookup provider error",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status502BadGateway);
            }
        });

        group.MapGet("/{id}", async (string id, ISearchRepository searches, CancellationToken ct) =>
        {
            var search = await searches.GetByIdAsync(id, ct);
            return search is null
                ? Results.NotFound()
                : Results.Ok(StartSearchHandler.ToDto(search));
        });

        group.MapGet("/{id}/businesses", async (
            string id,
            int? skip,
            int? take,
            ISearchRepository searches,
            IBusinessRepository businesses,
            CancellationToken ct) =>
        {
            var search = await searches.GetByIdAsync(id, ct);
            if (search is null)
            {
                return Results.NotFound();
            }

            var safeSkip = Math.Max(skip ?? 0, 0);
            var safeTake = Math.Clamp(take ?? 100, 1, 200);
            var items = await businesses.ListBySearchIdAsync(id, safeSkip, safeTake, ct);
            var total = await businesses.CountBySearchIdAsync(id, ct);

            return Results.Ok(new
            {
                items = items.Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    phone = b.Phone,
                    website = b.Website,
                    siteCreationYear = b.SiteCreationYear,
                    rating = b.Rating,
                    detailStatus = b.DetailStatus.ToString().ToLowerInvariant(),
                    detailError = b.DetailError
                }),
                total
            });
        });

        group.MapPost("/{id}/cancel", async (string id, CancelSearchHandler handler, CancellationToken ct) =>
        {
            try
            {
                var summary = await handler.HandleAsync(id, ct);
                return Results.Ok(summary);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(title: "Conflict", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        });

        group.MapGet("/{id}/export", async (string id, ExportSearchCsvHandler handler, CancellationToken ct) =>
        {
            try
            {
                var (fileName, content) = await handler.HandleAsync(id, ct);
                return Results.File(content, "text/csv; charset=utf-8", fileName);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Problem(title: "Conflict", detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        });

        return app;
    }
}
