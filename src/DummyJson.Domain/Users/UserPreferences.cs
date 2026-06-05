using System;
using DummyJson.Domain.Common.Primitives;

namespace DummyJson.Domain.Users;

/// <summary>
/// MongoDB entity storing a user's preferences.
/// Created automatically when a user registers.
/// </summary>
public sealed class UserPreferences : MongoEntity
{
    public Guid UserId { get; private set; }
    public string Theme { get; private set; } = "Light";
    public string Language { get; private set; } = "en-US";
    public bool ReceiveNewsletters { get; private set; } = true;
    public string? Image { get; private set; }

    private UserPreferences() { }

    public static UserPreferences Create(Guid userId, string theme = "Light", string language = "en-US", string? image = null)
    {
        return new UserPreferences
        {
            UserId = userId,
            Theme = theme,
            Language = language,
            Image = image
        };
    }

    public void UpdatePreferences(string theme, string language, bool receiveNewsletters, string? image)
    {
        Theme = theme;
        Language = language;
        ReceiveNewsletters = receiveNewsletters;
        Image = image;
    }
}
