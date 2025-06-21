using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp;

public static class Extensions
{
    /// <summary>
    /// Returns a human-friendly representation of a given <see cref="TimeSpan"/>.
    /// </summary>
    public static string ToReadableString(this TimeSpan span)
    {
        var parts = new StringBuilder();
        if (span.Days > 0)
            parts.Append($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")} ");
        if (span.Hours > 0)
            parts.Append($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")} ");
        if (span.Minutes > 0)
            parts.Append($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")} ");
        if (span.Seconds > 0)
            parts.Append($"{span.Seconds} second{(span.Seconds == 1 ? string.Empty : "s")} ");
        if (span.Milliseconds > 0)
            parts.Append($"{span.Milliseconds} millisecond{(span.Milliseconds == 1 ? string.Empty : "s")} ");

        if (parts.Length == 0) // result was less than 1 millisecond
            return $"{span.TotalMilliseconds:N4} milliseconds"; // similar to span.Ticks
        else
            return parts.ToString().Trim();
    }

    /// <summary>
    /// Gets the <see cref="TimeSpan"/> until tomorrow (at midnight) based on the current time.
    /// </summary>
    /// <param name="addHours">
    /// Optional hours to add to the midnight time, e.g. if you wanted a timespan to 1AM tomorrow, you would pass 1.
    /// </param>
    public static TimeSpan GetTimeSpanUntilMidnight(int addHours = 0)
    {
        if (addHours > 0)
            return DateTime.Today.AddDays(1).AddHours(addHours).Subtract(DateTime.Now);

        return DateTime.Today.AddDays(1).Subtract(DateTime.Now);
    }

    public static string TimeStampFormat(this DateTime dt) => dt.ToString("hh:mm:ss.fff tt");
}
