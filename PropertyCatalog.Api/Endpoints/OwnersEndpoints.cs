using MediatR;
using Microsoft.AspNetCore.Mvc;
using PropertyCatalog.Abstractions.Contracts.Owners;
using PropertyCatalog.Application.Owners.Commands.CreateOwner;
using Serilog;

namespace PropertyCatalog.Api.Endpoints;

public static class OwnersEndpoints
{
    public static IEndpointRouteBuilder MapOwnersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/owners").WithTags("Owners");

        group.MapPost("/", async (
            CreateOwnerRequest body,
            ISender mediator,
            CancellationToken ct) =>
        {            
            Log.Information(
                "CreateOwner llamada. Name={Name}, Address={Address}, Birthday={Birthday}",
                body.Name, body.Address, body.Birthday?.ToString("yyyy-MM-dd"));

            try
            {
                var cmd = new CreateOwnerCommand(
                    body.Name ?? string.Empty,
                    body.Address,
                    body.Photo,
                    body.Birthday
                );

                var created = await mediator.Send(cmd, ct);

                Log.Information("Owner creado correctamente. IdOwner={IdOwner}", created.IdOwner);

                return Results.Created($"/owners/{created.IdOwner}", created);
            }
            catch (ArgumentException ex)
            {                
                Log.Warning(ex, "Error de validación al crear Owner. Name={Name}", body.Name);

                var problem = new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Instance = "/owners",
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-400-bad-request"
                };
                return Results.Problem(problem);
            }
            catch (InvalidOperationException ex)
            {                
                Log.Warning(ex, "Conflicto al crear Owner. Name={Name}", body.Name);

                var problem = new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Instance = "/owners",
                    Type = "https://www.rfc-editor.org/rfc/rfc9110.html#name-409-conflict"
                };
                return Results.Problem(problem);
            }
            catch (Exception ex)
            {                
                Log.Error(ex, "Error inesperado al crear Owner. Name={Name}", body.Name);
                throw;
            }
        })
        .WithName("CreateOwner")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        return app;
    }
}
