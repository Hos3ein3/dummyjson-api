using DummyJson.Application.Common.Repository;
using DummyJson.Domain.Recipes;
using DummyJson.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Results;

namespace DummyJson.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRecipeRepository"/>.
/// Inherits all generic CRUD + bulk operations from <see cref="GenericRepository{TEntity,TId}"/>.
/// </summary>
public sealed class RecipeRepository : GenericRepository<Recipe, Guid>, IRecipeRepository
{
    public RecipeRepository(AppDbContext context) : base(context) { }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Recipe>> GetByCuisineAsync(
        string cuisine, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(r => EF.Functions.ILike(r.Cuisine, $"%{cuisine}%"))
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Recipe>> GetByDifficultyAsync(
        string difficulty, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(r => EF.Functions.ILike(r.Difficulty, difficulty))
            .OrderByDescending(r => r.Rating)
            .ToListAsync(ct);

    /// <inheritdoc/>
    public async Task<PagedList<Recipe>> SearchAsync(
        string? cuisine,
        string? difficulty,
        string? mealType,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(cuisine))
            query = query.Where(r => EF.Functions.ILike(r.Cuisine, $"%{cuisine}%"));

        if (!string.IsNullOrWhiteSpace(difficulty))
            query = query.Where(r => EF.Functions.ILike(r.Difficulty, difficulty));

        // MealType is a jsonb column — use Any operator for array containment
        if (!string.IsNullOrWhiteSpace(mealType))
            query = query.Where(r => r.MealType.Contains(mealType));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.Rating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<Recipe>(items, page, pageSize, total);
    }
}
