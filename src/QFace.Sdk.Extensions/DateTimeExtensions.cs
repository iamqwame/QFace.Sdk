namespace QFace.Sdk.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Checks if a date is between two dates (inclusive).
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>True if the date is between the start and end dates; otherwise, false.</returns>
    public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate)
    {
        return date >= startDate && date <= endDate;
    }
    
    /// <summary>
    /// Gets the first day of the month.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The first day of the month.</returns>
    public static DateTime FirstDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }
    
    /// <summary>
    /// Gets the last day of the month.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The last day of the month.</returns>
    public static DateTime LastDayOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
    
    /// <summary>
    /// Gets the next weekday.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="dayOfWeek">The day of the week to find.</param>
    /// <returns>The next weekday.</returns>
    public static DateTime NextWeekday(this DateTime date, DayOfWeek dayOfWeek)
    {
        int daysToAdd = ((int)dayOfWeek - (int)date.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7; // If it's the same day, get the next week
        return date.AddDays(daysToAdd);
    }
    
    /// <summary>
    /// Gets the previous weekday.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="dayOfWeek">The day of the week to find.</param>
    /// <returns>The previous weekday.</returns>
    public static DateTime PreviousWeekday(this DateTime date, DayOfWeek dayOfWeek)
    {
        int daysToSubtract = ((int)date.DayOfWeek - (int)dayOfWeek + 7) % 7;
        if (daysToSubtract == 0) daysToSubtract = 7; // If it's the same day, get the previous week
        return date.AddDays(-daysToSubtract);
    }
    
    /// <summary>
    /// Checks if a date is a weekday (Monday-Friday).
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is a weekday; otherwise, false.</returns>
    public static bool IsWeekday(this DateTime date)
    {
        return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
    }
    
    /// <summary>
    /// Checks if a date is a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is a weekend; otherwise, false.</returns>
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }
    
    /// <summary>
    /// Gets the start of the day (00:00:00).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The start of the day.</returns>
    public static DateTime StartOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
    }
    
    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The end of the day.</returns>
    public static DateTime EndOfDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
    }
    
    /// <summary>
    /// Gets the start of the week (Sunday by default).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="startOfWeek">The start day of the week.</param>
    /// <returns>The start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).StartOfDay();
    }
    
    /// <summary>
    /// Gets the end of the week (Saturday by default).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="startOfWeek">The start day of the week.</param>
    /// <returns>The end of the week.</returns>
    public static DateTime EndOfWeek(this DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
    {
        return date.StartOfWeek(startOfWeek).AddDays(6).EndOfDay();
    }
    
    /// <summary>
    /// Gets the number of days in the month.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The number of days in the month.</returns>
    public static int DaysInMonth(this DateTime date)
    {
        return DateTime.DaysInMonth(date.Year, date.Month);
    }
    
    /// <summary>
    /// Gets the number of days between two dates.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <returns>The number of days between the two dates.</returns>
    public static int DaysUntil(this DateTime startDate, DateTime endDate)
    {
        return (int)(endDate.Date - startDate.Date).TotalDays;
    }
    
    /// <summary>
    /// Gets the age based on the date of birth.
    /// </summary>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <returns>The age in years.</returns>
    public static int GetAge(this DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
    
    /// <summary>
    /// Formats a date as a relative time (e.g. "2 days ago", "in 3 hours").
    /// </summary>
    /// <param name="dateTime">The date to format.</param>
    /// <param name="referenceDate">The reference date (default is DateTime.Now).</param>
    /// <returns>A string representing the relative time.</returns>
    public static string ToRelativeTime(this DateTime dateTime, DateTime? referenceDate = null)
    {
        var reference = referenceDate ?? DateTime.Now;
        var ts = reference - dateTime;
        var delta = Math.Abs(ts.TotalSeconds);

        if (delta < 60)
        {
            return ts.Seconds == 1 
                ? ts.Seconds >= 0 ? "1 second ago" : "in 1 second" 
                : ts.Seconds >= 0 ? $"{ts.Seconds} seconds ago" : $"in {Math.Abs(ts.Seconds)} seconds";
        }
        if (delta < 3600)
        {
            int minutes = (int)Math.Floor(ts.TotalMinutes);
            return minutes == 1
                ? ts.TotalMinutes >= 0 ? "1 minute ago" : "in 1 minute"
                : ts.TotalMinutes >= 0 ? $"{minutes} minutes ago" : $"in {Math.Abs(minutes)} minutes";
        }
        if (delta < 86400)
        {
            int hours = (int)Math.Floor(ts.TotalHours);
            return hours == 1
                ? ts.TotalHours >= 0 ? "1 hour ago" : "in 1 hour"
                : ts.TotalHours >= 0 ? $"{hours} hours ago" : $"in {Math.Abs(hours)} hours";
        }
        if (delta < 2592000)
        {
            int days = (int)Math.Floor(ts.TotalDays);
            return days == 1
                ? ts.TotalDays >= 0 ? "1 day ago" : "in 1 day"
                : ts.TotalDays >= 0 ? $"{days} days ago" : $"in {Math.Abs(days)} days";
        }
        if (delta < 31104000)
        {
            int months = (int)Math.Floor(ts.TotalDays / 30);
            return months == 1
                ? ts.TotalDays >= 0 ? "1 month ago" : "in 1 month"
                : ts.TotalDays >= 0 ? $"{months} months ago" : $"in {Math.Abs(months)} months";
        }
        
        int years = (int)Math.Floor(ts.TotalDays / 365);
        return years == 1
            ? ts.TotalDays >= 0 ? "1 year ago" : "in 1 year"
            : ts.TotalDays >= 0 ? $"{years} years ago" : $"in {Math.Abs(years)} years";
    }
    
    /// <summary>
    /// Converts a DateTime to Unix timestamp (seconds since January 1, 1970).
    /// </summary>
    /// <param name="date">The date to convert.</param>
    /// <returns>The Unix timestamp.</returns>
    public static long ToUnixTimestamp(this DateTime date)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(date.ToUniversalTime() - epoch).TotalSeconds;
    }
    
    /// <summary>
    /// Converts a Unix timestamp to a DateTime.
    /// </summary>
    /// <param name="timestamp">The Unix timestamp (seconds since January 1, 1970).</param>
    /// <returns>The converted DateTime.</returns>
    public static DateTime FromUnixTimestamp(this long timestamp)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddSeconds(timestamp);
    }
}