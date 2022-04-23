using System;

namespace InventoryTools.Extensions
{
    public static class TimespanExtensions
    {
        public static string ToHumanReadableString (this TimeSpan t)
        {
            if (t.TotalMinutes <= 1) {
                return $@"{t:%s} seconds";
            }
            if (t.TotalHours <= 1) {
                return $@"{t:%m} minutes";
            }
            if (t.TotalDays <= 1) {
                return $@"{t:%h} hours";
            }

            return $@"{t:%d} days";
        }
    }
}