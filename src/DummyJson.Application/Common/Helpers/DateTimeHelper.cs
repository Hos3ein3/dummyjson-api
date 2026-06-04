using System;
using System.Globalization;

namespace DummyJson.Application.Common.Helpers;

public static class DateTimeHelper
{
    public static string ToPersianDateString(this DateTime date)
    {
        var pc = new PersianCalendar();
        return $"{pc.GetYear(date):0000}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00}";
    }

    public static string ToPersianDateString(this DateTime? date)
    {
        return date.HasValue ? date.Value.ToPersianDateString() : string.Empty;
    }

    public static string ToPersianDateTimeString(this DateTime date)
    {
        var pc = new PersianCalendar();
        return $"{pc.GetYear(date):0000}/{pc.GetMonth(date):00}/{pc.GetDayOfMonth(date):00} {date.ToString("HH:mm:ss")}";
    }

    public static DateTime StartOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);
    }

    public static DateTime EndOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Kind);
    }

    public static string GetTimeAgo(this DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        if (timeSpan <= TimeSpan.FromSeconds(60))
            return $"{timeSpan.Seconds} seconds ago";
        if (timeSpan <= TimeSpan.FromMinutes(60))
            return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "a minute ago";
        if (timeSpan <= TimeSpan.FromHours(24))
            return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "an hour ago";
        if (timeSpan <= TimeSpan.FromDays(30))
            return timeSpan.Days > 1 ? $"{timeSpan.Days} days ago" : "yesterday";
        if (timeSpan <= TimeSpan.FromDays(365))
            return timeSpan.Days > 30 ? $"{timeSpan.Days / 30} months ago" : "a month ago";

        return timeSpan.Days > 365 ? $"{timeSpan.Days / 365} years ago" : "a year ago";
    }
}
