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

    private UserPreferences() { }

    public static UserPreferences Create(Guid userId, string theme = "Light", string language = "en-US")
    {
        return new UserPreferences
        {
            UserId = userId,
            Theme = theme,
            Language = language
        };
    }

    public void UpdatePreferences(string theme, string language, bool receiveNewsletters)
    {
        Theme = theme;
        Language = language;
        ReceiveNewsletters = receiveNewsletters;
    }
}
