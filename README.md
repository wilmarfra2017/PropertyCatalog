# PropertyCatalog

API REST para gestionar y consultar un **catálogo de propiedades**.  
Construida con **.NET 8 Minimal API**, siguiendo **Clean Architecture**, patrón **CQRS** con **MediatR**, **MongoDB** como persistencia, **inyección de dependencias** nativa, **middleware** de manejo de excepciones y **logging** con **Serilog**. Incluye **OpenAPI/Swagger** y **pruebas unitarias** con **NUnit**.

--------------------------------------------------------------
## Tecnologías

- **.NET 8** – Minimal API  
- **CQRS** con **MediatR**  
- **MongoDB** (driver oficial)  
- **Serilog** (sink: consola) para registrar llamadas a endpoints, validaciones y warnings  
- **NUnit** (tests)  
- **Swagger / OpenAPI**  
- Middleware propio para manejo global de excepciones  
- Inyección de dependencias nativa  
- **CORS** habilitado para el front (`property-catalog-web`)


## La Api esta publicada en un App Service de Azure: 
https://apppropertycatalog-gbdwgxe2ckgactgm.eastus2-01.azurewebsites.net/swagger/index.html
## La base de datos de Mongo esta creada en un Cluster de MongoDB Atlas
- URL de conexion a la Base de datos para acceder por MongoDB Compass: 
-  **mongodb+srv://mart800749:Conslt$2024*@propertycatalog.so6eha5.mongodb.net/?retryWrites=true&w=majority&appName=PropertyCatalog**


## Arquitectura

El proyecto aplica **Clean Architecture** separando responsabilidades en capas:

- **Api**: Endpoints Minimal API, configuración de middleware, DI, Swagger, CORS.
- **Application**: Casos de uso (Handlers) siguiendo **CQRS** (Commands/Queries) con **MediatR**.
- **Domain**: Entidades, reglas y contratos agnósticos de infraestructura.
- **Abstractions**: Contratos (interfaces DTO/Repos), para desacoplar Application e Infrastructure.
- **Infrastructure**: Implementaciones de persistencia y repositorios (MongoDB).

```text
PropertyCatalog/
  ├─ PropertyCatalog.Api
  ├─ PropertyCatalog.Api.Tests
  ├─ PropertyCatalog.Application
  ├─ PropertyCatalog.Domain
  ├─ PropertyCatalog.Abstractions
  └─ PropertyCatalog.Infrastructure

#Estructura de carpetas:
  
PropertyCatalog.Api/
  Program.cs
  Middleware/ExceptionHandlingMiddleware.cs
  Endpoints/PropertiesEndpoints.cs
  Endpoints/OwnersEndpoints.cs

PropertyCatalog.Api.Tests/
  PropertiesEndpointsTests.cs

PropertyCatalog.Application/
  Properties/Queries/...
  Owners/Commands/...

PropertyCatalog.Infrastructure/
  Persistence/Mongo/
  Repositories/...

PropertyCatalog.Abstractions/
  Contracts/Properties/
  Contracts/Owners/
  Repositories/...

PropertyCatalog.Domain/
  Entities/...
````

## CORS

Se habilita CORS para permitir el front React property-catalog-web.
Ajustar el origen permitido según el entorno (ejemplo: http://localhost:5173).


## ENDPOINTS
- Búsqueda paginada y filtrada de propiedades.
- /properties (GET)

- https://apppropertycatalog-gbdwgxe2ckgactgm.eastus2-01.azurewebsites.net/properties


```text
Status 200 OK (Ejemplo)

{
  "items": [
    {
      "idProperty": "prop-1001",
      "name": "Cozy Loft",
      "address": "Main Ave 123",
      "price": 185000,
      "idOwner": "own-1",
      "mainImageUrl": "https://picsum.photos/id/1015/640/480"
    },
    {
      "idProperty": "prop-1002",
      "name": "Lake House",
      "address": "Lakeside 45",
      "price": 320000,
      "idOwner": "own-1",
      "mainImageUrl": "https://picsum.photos/id/1016/640/480"
    },
    {
      "idProperty": "prop-1003",
      "name": "Urban Studio",
      "address": "Downtown 9B",
      "price": 125000,
      "idOwner": "own-2",
      "mainImageUrl": "https://picsum.photos/id/1018/640/480"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "total": 3
}
````


- /properties/{id} (GET)
- https://apppropertycatalog-gbdwgxe2ckgactgm.eastus2-01.azurewebsites.net/properties/prop-1003

```text
Status 200 OK (Ejemplo)

{
  "idProperty": "prop-1002",
  "name": "Centro Comercial Estacion",
  "address": "Cra 8 n 55-21 Ibague",
  "price": 320000,
  "codeInternal": "PC-1002",
  "year": 2019,
  "owner": {
    "idOwner": "",
    "name": ""
  },
  "mainImageUrl": "https://i.pinimg.com/736x/72/d8/31/72d831e0e7a170448edf9014a91184c4.jpg",
  "otherImageUrls": [],
  "salesCount": 2,
  "lastSaleDate": "2024-01-10T00:00:00Z",
  "lastSaleValue": 320000
}
````

Se creó un endpoint adicional para almacenar Owners en BD Mongo
- /owners (POST)

```text
body:
{
  "name": "string",
  "address": "string",
  "photo": "string",
  "birthday": "2025-08-19T00:52:01.115Z"
}

Response
Status 201 OK (Ejemplo)

{
  "idOwner": "own-971e1275ce9d4d27a08a9f6f423649be",
  "name": "Anny Martinez"
}
````

## Test Unitarios realizados
![Vista de Swagger](TestUnitarios.png "Test Unitarios")