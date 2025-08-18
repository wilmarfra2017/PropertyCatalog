using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PropertyCatalog.Abstractions.Contracts.Properties;
using PropertyCatalog.Application.Properties.Queries.GetPropertyById;
using PropertyCatalog.Application.Properties.Queries.SearchProperties;
using Serilog;

namespace PropertyCatalog.Api.Endpoints;

public static class PropertiesEndpoints
{
    public static IEndpointRouteBuilder MapPropertiesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/properties").WithTags("Properties");

        group.MapGet("/", SearchProperties)
             .WithName("SearchProperties");


        group.MapGet("/{id}", async (string id, ISender mediator, CancellationToken ct) =>
        {
            Log.Information("GetPropertyById llamada. id={Id}", id);
            var result = await mediator.Send(new GetPropertyByIdQuery(id), ct);

            if (result is null)
            {
                Log.Warning("Propiedad no encontrada. id={Id}", id);

                var problem = new ProblemDetails
                {
                    Title = "Propiedad no encontrada",
                    Detail = $"La propiedad '{id}' no existe.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = $"/properties/{id}"
                };

                return Results.Problem(problem);
            }

            Log.Information("GetPropertyById funcionando. id={Id}", id);


            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetPropertyById");

        return app;
    }
    
    private static async Task<IResult> SearchProperties(
        string? name,
        string? address,
        decimal? priceMin,
        decimal? priceMax,
        int? yearMin,
        int? yearMax,
        string? ownerId,
        string? sortBy,
        string? sortDirection,
        int? page,
        int? pageSize,
        ISender mediator,
        CancellationToken ct)
    {
        LogRequest(name, address, priceMin, priceMax, yearMin, yearMax, ownerId, sortBy, sortDirection, page, pageSize);

        var (p, ps) = NormalizePaging(page, pageSize);

        var validationProblem = ValidateFilters(priceMin, priceMax, yearMin, yearMax, p, ps);
        if (validationProblem is not null) return validationProblem;

        var req = BuildSearchRequest(name, address, priceMin, priceMax, yearMin, yearMax, ownerId, sortBy, sortDirection, p, ps);

        try
        {
            var result = await mediator.Send(new SearchPropertiesQuery(req), ct);
            return Results.Ok(result);
        }
        catch (MongoCommandException ex) when (IsInvalidRegex(ex))
        {
            Log.Warning(ex, "Patrón de búsqueda inválido para name/address");
            return InvalidRegexProblem();
        }
        catch (OperationCanceledException)
        {
            Log.Warning("Solicitud cancelada por el cliente en /properties");
            return Results.StatusCode(StatusCodes.Status408RequestTimeout);
        }
    }
    
    private static void LogRequest(
        string? name, string? address,
        decimal? priceMin, decimal? priceMax,
        int? yearMin, int? yearMax,
        string? ownerId, string? sortBy, string? sortDirection,
        int? page, int? pageSize)
    {
        Log.Information(
            "SearchProperties llamada. name={Name}, address={Address}, priceMin={PriceMin}, priceMax={PriceMax}, " +
            "yearMin={YearMin}, yearMax={YearMax}, ownerId={OwnerId}, sortBy={SortBy}, sortDirection={SortDirection}, page={Page}, pageSize={PageSize}",
            name, address, priceMin, priceMax, yearMin, yearMax, ownerId, sortBy, sortDirection, page, pageSize);
    }

    private static (int Page, int PageSize) NormalizePaging(int? page, int? pageSize)
    {
        var p = page ?? 1;
        var ps = pageSize ?? 20;
        return (p, ps);
    }

    private static IResult? ValidateFilters(decimal? priceMin, decimal? priceMax, int? yearMin, int? yearMax, int page, int pageSize)
    {
        if (priceMin.HasValue && priceMax.HasValue && priceMin > priceMax)
            return Problem400("priceMin no puede ser mayor que priceMax.", "/properties");

        if (yearMin.HasValue && yearMax.HasValue && yearMin > yearMax)
            return Problem400("yearMin no puede ser mayor que yearMax.", "/properties");

        if (page <= 0 || pageSize <= 0 || pageSize > 100)
            return Problem400("page y pageSize deben ser positivos; pageSize <= 100.", "/properties");

        return null;
    }

    private static PropertySearchRequest BuildSearchRequest(
        string? name, string? address,
        decimal? priceMin, decimal? priceMax,
        int? yearMin, int? yearMax,
        string? ownerId, string? sortBy, string? sortDirection,
        int page, int pageSize)
        => new()
        {
            Name = name,
            Address = address,
            PriceMin = priceMin,
            PriceMax = priceMax,
            YearMin = yearMin,
            YearMax = yearMax,
            OwnerId = ownerId,
            SortBy = sortBy,
            SortDirection = sortDirection,
            Page = page,
            PageSize = pageSize
        };

    private static bool IsInvalidRegex(MongoCommandException ex) =>
        ex.Message.Contains("Regular expression is invalid", StringComparison.OrdinalIgnoreCase);

    private static IResult InvalidRegexProblem() =>
        Results.Problem(new ProblemDetails
        {
            Title = "Patrón de búsqueda inválido",
            Detail = "Alguno de los parámetros de texto contiene un patrón inválido para búsqueda.",
            Status = StatusCodes.Status400BadRequest,
            Instance = "/properties"
        });

    private static IResult Problem400(string detail, string instance) =>
        Results.Problem(new ProblemDetails
        {
            Title = "Parámetros inválidos",
            Detail = detail,
            Status = StatusCodes.Status400BadRequest,
            Instance = instance
        });
}
