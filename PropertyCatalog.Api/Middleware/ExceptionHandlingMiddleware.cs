using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using MongoDB.Driver;

namespace PropertyCatalog.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (MongoCommandException ex) when (ex.Message.Contains("Regular expression is invalid"))
        {
            _logger.LogWarning(ex, "Patrón de búsqueda inválido");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await Results.Problem(
                title: "Parámetro de búsqueda inválido",
                detail: "Alguno de los parámetros contiene caracteres de patrón no válidos.",
                statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
        }
        catch (OperationCanceledException oce) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogWarning(oce,
                "Solicitud cancelada por el cliente. {Method} {Path}",
                context.Request.Method, context.Request.Path);
            
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            _logger.LogError(ex,
                "Excepción no controlada. {Method} {Path} TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Ocurrió un error inesperado.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "El servidor encontró una condición inesperada que le impidió cumplir con la solicitud.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = traceId;

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers[HeaderNames.CacheControl] = "no-store";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
