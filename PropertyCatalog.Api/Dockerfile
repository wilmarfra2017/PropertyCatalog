FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PropertyCatalog.Api/PropertyCatalog.Api.csproj", "PropertyCatalog.Api/"]
COPY ["PropertyCatalog.Application/PropertyCatalog.Application.csproj", "PropertyCatalog.Application/"]
COPY ["PropertyCatalog.Domain/PropertyCatalog.Domain.csproj", "PropertyCatalog.Domain/"]
COPY ["PropertyCatalog.Infrastructure/PropertyCatalog.Infrastructure.csproj", "PropertyCatalog.Infrastructure/"]
COPY ["PropertyCatalog.Abstractions/PropertyCatalog.Abstractions.csproj", "PropertyCatalog.Abstractions/"]

RUN dotnet restore "PropertyCatalog.Api/PropertyCatalog.Api.csproj"

COPY . .
WORKDIR /src/PropertyCatalog.Api
RUN dotnet publish PropertyCatalog.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

ENV Mongo__ConnectionString="" \
    Mongo__Database="propertyCatalog"

EXPOSE 8080
ENTRYPOINT ["dotnet", "PropertyCatalog.Api.dll"]
