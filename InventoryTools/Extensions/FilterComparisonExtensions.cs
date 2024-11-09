using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Time;
using Dalamud.Utility;

namespace InventoryTools.Extensions
{


    public static class FilterComparisonExtensions
    {
        public class FilterComparisonText
        {
            public bool HasOr = false;
            public bool HasAnd = false;
            public bool StartsWithEquals = false;
            public bool StartsWithNegate = false;
            public bool StartsWithFuzzy = false;
            public string SearchText;

            public FilterComparisonText(string filterString)
            {
                SearchText = filterString.ToLower().Trim();
                if (filterString.Contains("||", StringComparison.Ordinal))
                {
                    HasOr = true;
                }
                if (filterString.Contains("&&", StringComparison.Ordinal))
                {
                    HasAnd = true;
                }
                if (filterString.StartsWith("=", StringComparison.Ordinal) && filterString.Length >= 2)
                {
                    StartsWithEquals = true;
                    SearchText = SearchText.Substring(1);
                }
                else if (filterString.StartsWith("!", StringComparison.Ordinal) && filterString.Length >= 2)
                {
                    StartsWithNegate = true;
                    SearchText = SearchText.Substring(1);
                }
                else if (filterString.StartsWith("~", StringComparison.Ordinal) && filterString.Length >= 2)
                {
                    StartsWithFuzzy = true;
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool PassesFilterComparisonText(this string text, FilterComparisonText filterString)
        {
            if (filterString.HasOr)
            {
                var ors = filterString.SearchText.Split("||");
                return ors.Select(text.PassesFilter).Any(c => c);
            }
            if (filterString.HasAnd)
            {
                var ands = filterString.SearchText.Split("&&");
                return ands.Select(text.PassesFilter).All(c => c);
            }
            if (filterString.StartsWithEquals)
            {
                if (text == filterString.SearchText)
                {
                    return true;
                }

                return false;
            }
            else if (filterString.StartsWithNegate)
            {
                return !text.Contains(filterString.SearchText);
            }
            else if (filterString.StartsWithFuzzy)
            {
                var filter = filterString.SearchText.Substring(1).Split(" ");
                var splitText = text.Split(" ");
                return filter.All(c => splitText.Any(d => d.Contains(c)));
            }

            return text.Contains(filterString.SearchText, StringComparison.Ordinal);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static bool PassesFilter(this TimeInterval timeInterval, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||", StringComparison.Ordinal))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(timeInterval, c)).Any(c => c);
            }
            if (filterString.Contains("&&", StringComparison.Ordinal))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(timeInterval, c)).All(c => c);
            }
            if (filterString.StartsWith("=", StringComparison.Ordinal) && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (timeInterval.ToString() == filter)
                {
                    return true;
                }

                if (TimeInterval.DurationString(timeInterval.End, TimeStamp.UtcNow, true) == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("<=") && filterString.Length >= 3)
            {

                var filter = filterString.Substring(2);
                if (int.TryParse(filter, CultureInfo.InvariantCulture, out var seconds))
                {
                    var timeStamp = timeInterval.Start - RealTime.MillisecondsPerSecond * seconds;
                    if (TimeStamp.UtcNow >= timeStamp)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("!", StringComparison.Ordinal))
            {
                if (filterString.Length >= 2)
                {
                    var filter = filterString.Substring(1);
                    return !TimeInterval.DurationString(timeInterval.End, TimeStamp.UtcNow, true).Contains(filter);
                }
                else if (filterString.Length == 1)
                {
                    return !TimeInterval.DurationString(timeInterval.End, TimeStamp.UtcNow, true).IsNullOrEmpty();
                }
            }
            else if (filterString.StartsWith("~", StringComparison.Ordinal) && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1).Split(" ");
                var splitText = TimeInterval.DurationString(timeInterval.End, TimeStamp.UtcNow, true).Split(" ");
                return filter.All(c => splitText.Any(d => d.Contains(c)));
            }

            return TimeInterval.DurationString(timeInterval.End, TimeStamp.UtcNow, true).Contains(filterString, StringComparison.Ordinal);
        }
    }
}