using Microsoft.Extensions.DependencyInjection;
using PropertyCatalog.Application.Owners;
using PropertyCatalog.Application.Properties;

namespace PropertyCatalog.Infrastructure.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddPropertyRepositories(this IServiceCollection services)
        => services
            .AddScoped<IPropertyReadRepository, PropertyReadRepository>()
            .AddScoped<IOwnerWriteRepository, OwnerWriteRepository>();
}
