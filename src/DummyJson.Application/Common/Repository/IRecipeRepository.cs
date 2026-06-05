using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Recipes;
using SharedKernel.Results;

using DummyJson.Application.Common.Interfaces;

namespace DummyJson.Application.Common.Repository;

/// <summary>
/// Strongly-typed repository for the <see cref="Recipe"/> aggregate root.
/// Extends <see cref="IMongoRepository{T}"/> with Recipe-specific queries.
/// </summary>
public interface IRecipeRepository : IMongoRepository<Recipe>
{
    /// <summary>Returns recipes filtered by cuisine (case-insensitive).</summary>
    Task<IReadOnlyList<Recipe>> GetByCuisineAsync(string cuisine, CancellationToken ct = default);

    /// <summary>Returns recipes filtered by difficulty level.</summary>
    Task<IReadOnlyList<Recipe>> GetByDifficultyAsync(string difficulty, CancellationToken ct = default);

    /// <summary>Returns recipes whose Tags or MealType contain any of the given values.</summary>
    Task<PagedList<Recipe>> SearchAsync(string? cuisine, string? difficulty, string? mealType, int page, int pageSize, CancellationToken ct = default);
}
