using DummyJson.Domain.Common.Primitives;
using SharedKernel.Results;

namespace DummyJson.Domain.Recipes;

/// <summary>
/// Recipe aggregate — corresponds to DummyJSON /recipes resource.
/// Stored in <b>MongoDB</b> as a single rich document.
///
/// <para>
/// All list-typed sub-documents (<see cref="Ingredients"/>, <see cref="Instructions"/>,
/// <see cref="Tags"/>, <see cref="MealType"/>) are embedded directly in the document
/// — no joins, no separate collections needed. This makes Recipes ideal for MongoDB:
/// the entire recipe is fetched in one round-trip and schema is flexible per-document.
/// </para>
///
/// <para>
/// Soft-delete and audit fields are stored as document properties.
/// There is no EF Core tracking; the Recipe collection is managed entirely via
/// <see cref="MongoDB.Driver.IMongoCollection{TDocument}"/>.
/// </para>
/// </summary>
public sealed class Recipe : MongoEntity
{
    private Recipe() { }   // Required for MongoDB driver deserialization

    private Recipe(
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
        int reviewCount)
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

    // ── Core properties ───────────────────────────────────────────────────────

    public string Name { get; private set; } = string.Empty;

    /// <summary>List of ingredient strings — embedded array in the MongoDB document.</summary>
    public List<string> Ingredients { get; private set; } = [];

    /// <summary>Step-by-step instructions — embedded array in the MongoDB document.</summary>
    public List<string> Instructions { get; private set; } = [];

    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }

    /// <summary>Difficulty level: Easy / Medium / Hard.</summary>
    public string Difficulty { get; private set; } = string.Empty;

    public string Cuisine { get; private set; } = string.Empty;
    public int CaloriesPerServing { get; private set; }

    /// <summary>Classification tags — embedded string array.</summary>
    public List<string> Tags { get; private set; } = [];

    /// <summary>Meal types (Breakfast / Lunch / Dinner / …) — embedded string array.</summary>
    public List<string> MealType { get; private set; } = [];

    public string? Image { get; private set; }
    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

    // ── Audit fields (stored as document properties) ───────────────────────────

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // ── Soft-delete fields ────────────────────────────────────────────────────

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
