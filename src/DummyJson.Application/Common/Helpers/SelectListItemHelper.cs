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

    public static List<SelectListItemHelper> FromEnum<TEnum>(IEnumerable<int>? selectedValues = null) where TEnum : struct, Enum
    {
        var dictionary = EnumHelper.GetEnumDictionary<TEnum>();
        var selectedSet = selectedValues != null ? new HashSet<int>(selectedValues) : new HashSet<int>();

        return dictionary.Select(x => new SelectListItemHelper(
            text: x.Value, 
            value: x.Key.ToString(), 
            selected: selectedSet.Contains(x.Key)
        )).ToList();
    }

    public static List<SelectListItemHelper> FromEnum<TEnum>(int selectedValue) where TEnum : struct, Enum
    {
        return FromEnum<TEnum>(new[] { selectedValue });
    }
}

public static class SelectListItemHelperExtensions
{
    /// <summary>
    /// Adds a default nullable option (e.g. "Please select") to the beginning of the list.
    /// Useful for nullable dropdowns.
    /// </summary>
    public static List<SelectListItemHelper> WithDefaultOption(
        this List<SelectListItemHelper> list, 
        string text = "--- Select ---", 
        string value = "")
    {
        list.Insert(0, new SelectListItemHelper(text, value, selected: string.IsNullOrEmpty(value)));
        return list;
    }
}
