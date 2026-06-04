using DummyJson.Domain.Common.Interfaces;
using SharedKernel.Results;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Recipes;

/// <summary>
/// Recipe aggregate root — corresponds to DummyJSON /recipes resource.
/// Stored in PostgreSQL via EF Core.
/// Ingredients, instructions, tags and meal-type lists are stored as
/// JSON columns (PostgreSQL jsonb / SQL Server nvarchar JSON).
/// </summary>
public sealed class Recipe : AggregateRoot<Guid>, IAuditable, ISoftDelete
{
    private Recipe() { }

    private Recipe(
        Guid id,
        string name,
        List<string> ingredients,
        List<string> instructions,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        string difficulty,
        string cuisine,
        int caloriesPerServing,
        List<string> tags,
        List<string> mealType,
        string? image,
        double rating,
        int reviewCount) : base(id)
    {
        Name = name;
        Ingredients = ingredients;
        Instructions = instructions;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        Difficulty = difficulty;
        Cuisine = cuisine;
        CaloriesPerServing = caloriesPerServing;
        Tags = tags;
        MealType = mealType;
        Image = image;
        Rating = rating;
        ReviewCount = reviewCount;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>List of ingredients. Stored as JSON column.</summary>
    public List<string> Ingredients { get; private set; } = [];

    /// <summary>Step-by-step instructions. Stored as JSON column.</summary>
    public List<string> Instructions { get; private set; } = [];

    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }

    /// <summary>Difficulty level: Easy / Medium / Hard.</summary>
    public string Difficulty { get; private set; } = string.Empty;

    public string Cuisine { get; private set; } = string.Empty;
    public int CaloriesPerServing { get; private set; }

    /// <summary>Classification tags. Stored as JSON column.</summary>
    public List<string> Tags { get; private set; } = [];

    /// <summary>Meal types (Breakfast / Lunch / Dinner / …). Stored as JSON column.</summary>
    public List<string> MealType { get; private set; } = [];

    public string? Image { get; private set; }
    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

    // IAuditable
    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // ── Factory ───────────────────────────────────────────────────────────────

    public static Result<Recipe> Create(
        string name,
        List<string> ingredients,
        List<string> instructions,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        string difficulty,
        string cuisine,
        int caloriesPerServing,
        List<string> tags,
        List<string> mealType,
        string? image = null,
        double rating = 0,
        int reviewCount = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Recipe>(Error.Validation(nameof(name), "Recipe name cannot be empty."));

        if (ingredients.Count == 0)
            return Result.Failure<Recipe>(Error.Validation(nameof(ingredients), "Ingredients cannot be empty."));

        return Result.Success(new Recipe(
            Guid.CreateVersion7(),
            name, ingredients, instructions,
            prepTimeMinutes, cookTimeMinutes, servings,
            difficulty, cuisine, caloriesPerServing,
            tags, mealType, image, rating, reviewCount));
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    public Result UpdateDetails(string name, int prepTimeMinutes, int cookTimeMinutes, int servings)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation(nameof(name), "Recipe name cannot be empty."));

        Name = name;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        Servings = servings;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void UpdateImage(string? image)
    {
        Image = image;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
