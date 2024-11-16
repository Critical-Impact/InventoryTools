using System;

namespace InventoryTools.Extensions
{
    public static class TimespanExtensions
    {
        public static string ToHumanReadableString (this TimeSpan t)
        {
            if (t.TotalMinutes <= 1) {
                return $@"{t.TotalSeconds} seconds";
            }
            if (t.TotalHours <= 1) {
                return $@"{t.TotalMinutes} minutes";
            }
            if (t.TotalDays <= 1) {
                return $@"{t.TotalHours} hours";
            }

            return $@"{t.TotalDays} days";
        }
    }
}