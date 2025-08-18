using Microsoft.AspNetCore.HttpLogging;
using Microsoft.OpenApi.Models;
using PropertyCatalog.Api.Endpoints;
using PropertyCatalog.Api.Middleware;
using PropertyCatalog.Application;
using PropertyCatalog.Infrastructure.Persistence.Mongo;
using PropertyCatalog.Infrastructure.Repositories;
using Serilog;
using System.Diagnostics;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestPropertiesAndHeaders |
                      HttpLoggingFields.ResponsePropertiesAndHeaders |
                      HttpLoggingFields.RequestBody |
                      HttpLoggingFields.ResponseBody;
    o.RequestBodyLogLimit = 1024 * 4;
    o.ResponseBodyLogLimit = 1024 * 4;
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        if (ctx.HttpContext is { } http)
            ctx.ProblemDetails.Extensions["traceId"] =
                Activity.Current?.Id ?? http.TraceIdentifier;
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "PropertyCatalog.Api", Version = "v1" });
});

builder.Services.AddMongo(builder.Configuration).AddMongoSimpleSetup();
builder.Services.AddPropertyRepositories();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

var app = builder.Build();

app.UseSwagger(c =>
{
    c.RouteTemplate = "openapi/{documentName}/openapi.json";
});

app.UseSwaggerUI(c =>
{
    c.RoutePrefix = "swagger";
    c.SwaggerEndpoint($"/openapi/v1/openapi.json?v={Guid.NewGuid()}", "PropertyCatalog.Api v1");
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapPropertiesEndpoints();
app.MapOwnersEndpoints();
app.Run();
