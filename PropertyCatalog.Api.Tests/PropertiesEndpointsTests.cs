using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PropertyCatalog.Abstractions.Contracts.Owners;
using PropertyCatalog.Abstractions.Contracts.Properties;
using PropertyCatalog.Abstractions.Primitives;
using PropertyCatalog.Api.Endpoints;
using PropertyCatalog.Application.Properties.Queries.GetPropertyById;
using PropertyCatalog.Application.Properties.Queries.SearchProperties;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PropertyCatalog.Api.Tests;

[TestFixture]
public class PropertiesEndpointsTests
{
    private static HttpClient CreateClientWith(ISender sender)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.AddEndpointsApiExplorer();
                services.AddSingleton(sender);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapPropertiesEndpoints();
                });
            });

        var server = new TestServer(builder);
        return server.CreateClient();
    }

    private static readonly JsonSerializerOptions Json =
        new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task SearchProperties_returns_200_and_payload()
    {        
        var expected = new PagedResult<PropertyListItemDto>
        {
            Items = new List<PropertyListItemDto>
            {
                new PropertyListItemDto
                {
                    IdProperty = "prop-1001",
                    Name = "Propiedad Prefabricada 1",
                    Address = "Cra 5 no 8 - 99",
                    Price = 185000m,
                    IdOwner = "own-1",
                    MainImageUrl = "https://co.pinterest.com/pin/392024342537733955/"
                }
            },
            Page = 1,
            PageSize = 20,
            Total = 1
        };

        var mediator = new Mock<ISender>();
        mediator
            .Setup(m => m.Send(It.IsAny<SearchPropertiesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        using var client = CreateClientWith(mediator.Object);
        
        var response = await client.GetAsync("/properties?name=Propiedad Prefabricada 1");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var actual = await response.Content.ReadFromJsonAsync<PagedResult<PropertyListItemDto>>(Json);
        actual.Should().NotBeNull();
        actual!.Total.Should().Be(1);
        actual.Items.Should().HaveCount(1);
        actual.Items[0].IdProperty.Should().Be("prop-1001");
        
        mediator.Verify(m =>
            m.Send(It.IsAny<SearchPropertiesQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetPropertyById_returns_200_when_found()
    {        
        var dto = new PropertyDetailDto
        {
            IdProperty = "prop-1002",
            Name = "Propiedad Prefabricada 2",
            Address = "Cra 3 no 97",
            Price = 320000m,
            CodeInternal = "PC-1002",
            Year = 2019,
            Owner = new OwnerSummaryDto { IdOwner = "own-1", Name = "Propiedad Prefabricada 2" },
            MainImageUrl = "https://co.pinterest.com/pin/392024342537733923/"
        };

        var mediator = new Mock<ISender>();
        mediator
            .Setup(m => m.Send(
                It.Is<GetPropertyByIdQuery>(q => q.IdProperty == "prop-1002"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        using var client = CreateClientWith(mediator.Object);
        
        var response = await client.GetAsync("/properties/prop-1002");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PropertyDetailDto>(Json);
        body!.IdProperty.Should().Be("prop-1002");

        mediator.VerifyAll();
    }

    [Test]
    public async Task GetPropertyById_returns_404_when_not_found()
    {        
        var mediator = new Mock<ISender>();
        mediator
            .Setup(m => m.Send(
                It.Is<GetPropertyByIdQuery>(q => q.IdProperty == "does-not-exist"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PropertyDetailDto?)null);

        using var client = CreateClientWith(mediator.Object);
        
        var response = await client.GetAsync("/properties/does-not-exist");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        mediator.VerifyAll();
    }
}
