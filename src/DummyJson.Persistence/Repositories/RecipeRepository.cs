using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Recipes;
using DummyJson.Persistence.Context;
using MongoDB.Driver;
using MongoDB.Bson;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of <see cref="IRecipeRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="MongoRepository{T}"/>.
/// </summary>
public sealed class RecipeRepository : MongoRepository<Recipe>, IRecipeRepository
{
    private readonly IMongoCollection<Recipe> _recipes;

    public RecipeRepository(MongoDbContext context) : base(context) 
    {
        _recipes = context.Recipes;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Recipe>> GetByCuisineAsync(
        string cuisine, CancellationToken ct = default)
    {
        var filter = Builders<Recipe>.Filter.Regex(r => r.Cuisine, new BsonRegularExpression(cuisine, "i"));
        return await _recipes.Find(filter).SortBy(r => r.Name).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Recipe>> GetByDifficultyAsync(
        string difficulty, CancellationToken ct = default)
    {
        var filter = Builders<Recipe>.Filter.Eq(r => r.Difficulty, difficulty);
        return await _recipes.Find(filter).SortByDescending(r => r.Rating).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PagedList<Recipe>> SearchAsync(
        string? cuisine,
        string? difficulty,
        string? mealType,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var builder = Builders<Recipe>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(cuisine))
            filter &= builder.Regex(r => r.Cuisine, new BsonRegularExpression(cuisine, "i"));

        if (!string.IsNullOrWhiteSpace(difficulty))
            filter &= builder.Eq(r => r.Difficulty, difficulty);

        if (!string.IsNullOrWhiteSpace(mealType))
            filter &= builder.AnyEq(r => r.MealType, mealType);

        var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<Recipe, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<Recipe>()
            }));

        var dataFacet = AggregateFacet.Create("data",
            PipelineDefinition<Recipe, Recipe>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Sort(Builders<Recipe>.Sort.Descending(r => r.Rating)),
                PipelineStageDefinitionBuilder.Skip<Recipe>((page - 1) * pageSize),
                PipelineStageDefinitionBuilder.Limit<Recipe>(pageSize)
            }));

        var aggregation = await _recipes.Aggregate()
            .Match(filter)
            .Facet(countFacet, dataFacet)
            .ToListAsync(ct);

        var count = aggregation.First()
            .Facets.First(x => x.Name == "count")
            .Output<AggregateCountResult>()
            ?.FirstOrDefault()?.Count ?? 0;

        var items = aggregation.First()
            .Facets.First(x => x.Name == "data")
            .Output<Recipe>()
            .ToList();

        return new PagedList<Recipe>(items, page, pageSize, (int)count);
    }
}
