namespace DummyJson.Application.Common.Helpers;

public class SelectListItemHelper
{
    public string Text { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Selected { get; set; }

    public SelectListItemHelper() { }

    public SelectListItemHelper(string text, string value, bool selected = false)
    {
        Text = text;
        Value = value;
        Selected = selected;
    }

    public static List<SelectListItemHelper> FromEnum<TEnum>() where TEnum : struct, Enum
    {
        var dictionary = EnumHelper.GetEnumDictionary<TEnum>();
        return dictionary.Select(x => new SelectListItemHelper(x.Value, x.Key.ToString())).ToList();
    }
}
