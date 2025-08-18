using MongoDB.Bson;
using MongoDB.Driver;
using PropertyCatalog.Abstractions.Contracts.Properties;
using PropertyCatalog.Abstractions.Primitives;
using PropertyCatalog.Application.Properties;
using System.Text.RegularExpressions;

namespace PropertyCatalog.Infrastructure.Repositories;

public sealed class PropertyReadRepository : IPropertyReadRepository
{
    private readonly IMongoDatabase _db;

    public PropertyReadRepository(IMongoDatabase db) => _db = db;

    public async Task<PagedResult<PropertyListItemDto>> SearchAsync(
        PropertySearchRequest request, CancellationToken ct)
    {
        var properties = _db.GetCollection<BsonDocument>("properties");

        var fb = Builders<BsonDocument>.Filter;
        var filters = new List<FilterDefinition<BsonDocument>>();

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var pattern = Regex.Escape(request.Name.Trim());
            filters.Add(fb.Regex("name", new BsonRegularExpression(pattern, "i")));
        }

        if (!string.IsNullOrWhiteSpace(request.Address))
        {
            var pattern = Regex.Escape(request.Address.Trim());
            filters.Add(fb.Regex("address", new BsonRegularExpression(pattern, "i")));
        }
            

        if (request.PriceMin.HasValue)
            filters.Add(fb.Gte("price", new BsonDecimal128(request.PriceMin.Value)));

        if (request.PriceMax.HasValue)
            filters.Add(fb.Lte("price", new BsonDecimal128(request.PriceMax.Value)));

        if (request.YearMin.HasValue)
            filters.Add(fb.Gte("year", request.YearMin.Value));

        if (request.YearMax.HasValue)
            filters.Add(fb.Lte("year", request.YearMax.Value));

        if (!string.IsNullOrWhiteSpace(request.OwnerId))
            filters.Add(fb.Eq("idOwner", request.OwnerId));

        var filter = filters.Count > 0 ? fb.And(filters) : FilterDefinition<BsonDocument>.Empty;

        var total = await properties.CountDocumentsAsync(filter, cancellationToken: ct);

        var sortBy = (request.SortBy ?? "name").Trim().ToLowerInvariant();
        var sortDir = (request.SortDirection ?? "asc").Trim().ToLowerInvariant();
        var sb = Builders<BsonDocument>.Sort;

        SortDefinition<BsonDocument> sort = sortBy switch
        {
            "price" => sortDir == "desc" ? sb.Descending("price") : sb.Ascending("price"),
            "year" => sortDir == "desc" ? sb.Descending("year") : sb.Ascending("year"),
            "name" => sortDir == "desc" ? sb.Descending("name") : sb.Ascending("name"),
            _ => sb.Ascending("name")
        };

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var skip = (page - 1) * pageSize;

        var pipeline = new List<PipelineStageDefinition<BsonDocument, BsonDocument>>
        {
            PipelineStageDefinitionBuilder.Match(filter),
            PipelineStageDefinitionBuilder.Sort(sort),
            PipelineStageDefinitionBuilder.Skip<BsonDocument>(skip),
            PipelineStageDefinitionBuilder.Limit<BsonDocument>(pageSize),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "owners" },
                { "localField", "idOwner" },
                { "foreignField", "_id" },
                { "as", "owner" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$owner" },
                { "preserveNullAndEmptyArrays", true }
            }),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "propertyImages" },
                { "let", new BsonDocument("propId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray {"$idProperty", "$$propId"}),
                            new BsonDocument("$eq", new BsonArray {"$enabled", true})
                        }))),
                        new BsonDocument("$limit", 1)
                    }
                },
                { "as", "mainImage" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$mainImage" },
                { "preserveNullAndEmptyArrays", true }
            }),
            
            new BsonDocument("$project", new BsonDocument
            {
                { "idProperty", "$_id" },
                { "name", 1 },
                { "address", 1 },
                { "price", 1 },
                { "idOwner", 1 },
                { "ownerName", "$owner.name" },
                { "mainImageUrl", "$mainImage.file" }
            })
        };

        var pipelineDef = PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline);
        var docs = await properties.Aggregate<BsonDocument>(pipelineDef).ToListAsync(ct);

        var items = docs.Select(d => new PropertyListItemDto
        {
            IdProperty = d.GetValue("idProperty").AsString,
            Name = d.GetValue("name").AsString,
            Address = d.GetValue("address").AsString,
            Price = d.GetValue("price").ToDecimal(),
            IdOwner = d.TryGetValue("idOwner", out var io) && !io.IsBsonNull ? io.AsString : null,            
            MainImageUrl = d.TryGetValue("mainImageUrl", out var mi) && !mi.IsBsonNull ? mi.AsString : null
        }).ToList();

        return new PagedResult<PropertyListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<PropertyDetailDto?> GetByIdAsync(string idProperty, CancellationToken ct)
    {
        var properties = _db.GetCollection<BsonDocument>("properties");

        var pipeline = new List<PipelineStageDefinition<BsonDocument, BsonDocument>>
        {
            PipelineStageDefinitionBuilder.Match(
                Builders<BsonDocument>.Filter.Eq("_id", idProperty)
            ),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "owners" },
                { "localField", "idOwner" },
                { "foreignField", "_id" },
                { "as", "owner" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$owner" },
                { "preserveNullAndEmptyArrays", true }
            }),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "propertyImages" },
                { "let", new BsonDocument("propId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray {"$idProperty", "$$propId"}),
                            new BsonDocument("$eq", new BsonArray {"$enabled", true})
                        }))),
                        new BsonDocument("$limit", 1)
                    }
                },
                { "as", "mainImage" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$mainImage" },
                { "preserveNullAndEmptyArrays", true }
            }),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "propertyImages" },
                { "let", new BsonDocument("propId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$and", new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray { "$idProperty", "$$propId" }),
                            new BsonDocument("$ne", new BsonArray { "$enabled", true })
                        }))),
                        new BsonDocument("$project", new BsonDocument { { "file", 1 } })
                    }
                },
                { "as", "otherImages" }
            }),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "propertyTraces" },
                { "let", new BsonDocument("propId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray
                        {
                            "$idProperty", "$$propId"
                        }))),
                        new BsonDocument("$sort", new BsonDocument("dateSale", -1)),
                        new BsonDocument("$limit", 1)
                    }
                },
                { "as", "lastTrace" }
            }),
            new BsonDocument("$unwind", new BsonDocument
            {
                { "path", "$lastTrace" },
                { "preserveNullAndEmptyArrays", true }
            }),
            
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "propertyTraces" },
                { "let", new BsonDocument("propId", "$_id") },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$match", new BsonDocument("$expr", new BsonDocument("$eq", new BsonArray
                        {
                            "$idProperty", "$$propId"
                        }))),
                        new BsonDocument("$group", new BsonDocument
                        {
                            { "_id", BsonNull.Value },
                            { "count", new BsonDocument("$sum", 1) }
                        })
                    }
                },
                { "as", "tracesAgg" }
            }),
            
            new BsonDocument("$project", new BsonDocument
            {
                { "idProperty", "$_id" },
                { "name", 1 },
                { "address", 1 },
                { "price", 1 },
                { "codeInternal", 1 },
                { "year", 1 },
                { "owner", new BsonDocument
                    {
                        { "idOwner", "$owner._id" },
                        { "name", "$owner.name" }
                    }
                },
                { "mainImageUrl", "$mainImage.file" },
                { "otherImageUrls", new BsonDocument("$map", new BsonDocument
                    {
                        { "input", "$otherImages" },
                        { "as", "img" },
                        { "in", "$$img.file" }
                    })
                },
                { "lastSaleDate", "$lastTrace.dateSale" },
                { "lastSaleValue", "$lastTrace.value" },
                { "salesCount", new BsonDocument("$ifNull", new BsonArray
                    {
                        new BsonDocument("$first", "$tracesAgg.count"),
                        0
                    })
                }
            })
        };

        var pipelineDef = PipelineDefinition<BsonDocument, BsonDocument>.Create(pipeline);
        var doc = await properties.Aggregate<BsonDocument>(pipelineDef).FirstOrDefaultAsync(ct);
        if (doc is null) return null;

        return new PropertyDetailDto
        {
            IdProperty = doc["idProperty"].AsString,
            Name = doc["name"].AsString,
            Address = doc["address"].AsString,
            Price = doc["price"].ToDecimal(),
            CodeInternal = doc["codeInternal"].AsString,
            Year = doc["year"].ToInt32(),
            Owner = doc.TryGetValue("owner", out var ow) && ow.IsBsonDocument
                                ? new PropertyCatalog.Abstractions.Contracts.Owners.OwnerSummaryDto
                                {
                                    IdOwner = ow.AsBsonDocument.GetValue("idOwner", BsonNull.Value).IsBsonNull
                                                ? ""
                                                : ow.AsBsonDocument["idOwner"].AsString,
                                    Name = ow.AsBsonDocument.GetValue("name", BsonNull.Value).IsBsonNull
                                                ? ""
                                                : ow.AsBsonDocument["name"].AsString
                                }
                                : null,
            MainImageUrl = doc.TryGetValue("mainImageUrl", out var mi) && !mi.IsBsonNull ? mi.AsString : null,
            OtherImageUrls = doc.TryGetValue("otherImageUrls", out var oi) && oi.IsBsonArray
                                ? oi.AsBsonArray.Where(v => !v.IsBsonNull).Select(v => v.AsString).ToList()
                                : null,
            SalesCount = doc.TryGetValue("salesCount", out var sc) && !sc.IsBsonNull ? sc.ToInt32() : (int?)null,
            LastSaleDate = doc.TryGetValue("lastSaleDate", out var lsd) && !lsd.IsBsonNull ? lsd.ToUniversalTime() : (DateTime?)null,
            LastSaleValue = doc.TryGetValue("lastSaleValue", out var lsv) && !lsv.IsBsonNull ? lsv.ToDecimal() : (decimal?)null
        };
    }
}
