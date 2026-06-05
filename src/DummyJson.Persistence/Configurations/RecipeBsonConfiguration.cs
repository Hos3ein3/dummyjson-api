using DummyJson.Domain.Recipes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace DummyJson.Persistence.Configurations;

/// <summary>
/// MongoDB BSON class-map configuration for <see cref="Recipe"/>.
///
/// <para>
/// Recipes are stored as rich MongoDB documents in the <c>recipes</c> collection.
/// All list properties (<c>ingredients</c>, <c>instructions</c>, <c>tags</c>,
/// <c>mealType</c>) are embedded arrays in the document — no joins required.
/// </para>
///
/// <para>
/// Audit (<c>createdAt</c>, <c>updatedAt</c>, …) and soft-delete (<c>isDeleted</c>, …)
/// fields are stored as plain document properties.
/// </para>
///
/// <para>Register once at startup via <see cref="Register"/>.</para>
/// </summary>
public static class RecipeBsonConfiguration
{
    private static bool _registered;
    private static readonly Lock _lock = new();

    /// <summary>
    /// Registers the BSON class map for <see cref="Recipe"/>.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
    public static void Register()
    {
        lock (_lock)
        {
            if (_registered) return;

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreIfNullConvention(true),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("DummyJson.Recipe", pack, t => t == typeof(Recipe));

            if (!BsonClassMap.IsClassMapRegistered(typeof(Recipe)))
            {
                BsonClassMap.RegisterClassMap<Recipe>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);

                    // ── Core scalar fields ────────────────────────────────────
                    cm.MapMember(r => r.Name).SetElementName("name").SetIsRequired(true);
                    cm.MapMember(r => r.PrepTimeMinutes).SetElementName("prepTimeMinutes");
                    cm.MapMember(r => r.CookTimeMinutes).SetElementName("cookTimeMinutes");
                    cm.MapMember(r => r.Servings).SetElementName("servings");
                    cm.MapMember(r => r.Difficulty).SetElementName("difficulty");
                    cm.MapMember(r => r.Cuisine).SetElementName("cuisine");
                    cm.MapMember(r => r.CaloriesPerServing).SetElementName("caloriesPerServing");
                    cm.MapMember(r => r.Rating).SetElementName("rating");
                    cm.MapMember(r => r.ReviewCount).SetElementName("reviewCount");
                    cm.MapMember(r => r.Image).SetElementName("image");

                    // ── Embedded arrays ───────────────────────────────────────
                    cm.MapMember(r => r.Ingredients).SetElementName("ingredients");
                    cm.MapMember(r => r.Instructions).SetElementName("instructions");
                    cm.MapMember(r => r.Tags).SetElementName("tags");
                    cm.MapMember(r => r.MealType).SetElementName("mealType");

                    // ── Audit fields ──────────────────────────────────────────
                    cm.MapMember(r => r.CreatedAt).SetElementName("createdAt")
                        .SetSerializer(new DateTimeOffsetSerializer(BsonType.DateTime));
                    cm.MapMember(r => r.UpdatedAt).SetElementName("updatedAt")
                        .SetSerializer(new NullableSerializer<DateTimeOffset>(
                            new DateTimeOffsetSerializer(BsonType.DateTime)));
                    cm.MapMember(r => r.CreatedBy).SetElementName("createdBy");
                    cm.MapMember(r => r.UpdatedBy).SetElementName("updatedBy");

                    // ── Soft-delete fields ────────────────────────────────────
                    cm.MapMember(r => r.IsDeleted).SetElementName("isDeleted").SetDefaultValue(false);
                    cm.MapMember(r => r.DeletedAt).SetElementName("deletedAt")
                        .SetSerializer(new NullableSerializer<DateTimeOffset>(
                            new DateTimeOffsetSerializer(BsonType.DateTime)));
                    cm.MapMember(r => r.DeletedBy).SetElementName("deletedBy");
                });
            }

            _registered = true;
        }
    }
}
