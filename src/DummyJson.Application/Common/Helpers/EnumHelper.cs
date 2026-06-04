using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DummyJson.Application.Common.Helpers;

public static class EnumHelper
{
    public static string GetDisplayValue(Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        if (fieldInfo is null) return value.ToString();

        var descriptionAttributes = fieldInfo.GetCustomAttributes(
            typeof(DisplayAttribute), false) as DisplayAttribute[];

        if (descriptionAttributes is null || descriptionAttributes.Length == 0) return value.ToString();
        if (descriptionAttributes[0].ResourceType != null)
            return descriptionAttributes[0].GetName() ?? value.ToString();
        
        return descriptionAttributes[0].Name ?? value.ToString();
    }

    public static Dictionary<int, string> GetEnumDictionary<TEnum>() where TEnum : struct, Enum
    {
        var dictionary = new Dictionary<int, string>();
        foreach (TEnum e in Enum.GetValues(typeof(TEnum)))
        {
            dictionary.Add(Convert.ToInt32(e), GetDisplayValue(e));
        }
        return dictionary;
    }
}
