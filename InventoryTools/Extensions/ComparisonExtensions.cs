using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Game.Text;
using Dalamud.Utility;

namespace InventoryTools.Extensions
{
    public static class ComparisonExtensions
    {
        public class FilterComparisonText
        {
            public bool HasOr = false;
            public bool HasAnd = false;
            public bool StartsWithEquals = false;
            public bool StartsWithNegate = false;
            public bool StartsWithFuzzy = false;
            public string SearchText;

            //TODO: Allow this to have sub comparisons
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
        public static bool PassesFilter(this string text, FilterComparisonText filterString)
        {
            if (filterString.HasOr)
            {
                var ors = filterString.SearchText.Split("||");
                return ors.Select(c => PassesFilter(text, c)).Any(c => c);
            }
            if (filterString.HasAnd)
            {
                var ands = filterString.SearchText.Split("&&");
                return ands.Select(c => PassesFilter(text, c)).All(c => c);
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
        public static bool PassesFilter(this string text, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||", StringComparison.Ordinal))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(text, c)).Any(c => c);
            }
            if (filterString.Contains("&&", StringComparison.Ordinal))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(text, c)).All(c => c);
            }
            if (filterString.StartsWith("=", StringComparison.Ordinal) && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (text == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("!", StringComparison.Ordinal))
            {
                if (filterString.Length >= 2)
                {
                    var filter = filterString.Substring(1);
                    return !text.Contains(filter);
                }
                else if (filterString.Length == 1)
                {
                    return !text.IsNullOrEmpty();
                }
            }
            else if (filterString.StartsWith("~", StringComparison.Ordinal) && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1).Split(" ");
                var splitText = text.Split(" ");
                return filter.All(c => splitText.Any(d => d.Contains(c)));
            }

            return text.Contains(filterString, StringComparison.Ordinal);
        }

        public static bool PassesFilter(this uint number, string filterString)
        {
            return PassesFilter((int) number, filterString);
        }

        public static bool PassesFilter(this ushort number, string filterString)
        {
            return PassesFilter((int) number, filterString);
        }
        
        public static bool PassesFilter(this double number, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||"))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(number, c)).Any(c => c);
            }
            if (filterString.Contains("&&"))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(number, c)).All(c => c);
            }
            if (filterString.StartsWith("=") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (number.ToString() == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("<=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult >= number)
                    {
                        return true;
                    }
                }
            }
            else if(filterString.StartsWith(">=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult <= number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("<") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult > number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith(">") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult < number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("!") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                return !number.ToString().Contains(filter);
            }

            return number.ToString().Contains(filterString);
        }
        
        public static bool PassesFilter(this decimal number, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||"))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(number, c)).Any(c => c);
            }
            if (filterString.Contains("&&"))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(number, c)).All(c => c);
            }
            if (filterString.StartsWith("=") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (number.ToString() == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("<=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (decimal.TryParse(filter, out var numberResult))
                {
                    if (numberResult >= number)
                    {
                        return true;
                    }
                }
            }
            else if(filterString.StartsWith(">=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (decimal.TryParse(filter, out var numberResult))
                {
                    if (numberResult <= number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("<") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (decimal.TryParse(filter, out var numberResult))
                {
                    if (numberResult > number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith(">") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (decimal.TryParse(filter, out var numberResult))
                {
                    if (numberResult < number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("!") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                return !number.ToString().Contains(filter);
            }

            return number.ToString().Contains(filterString);
        }
        
        public static bool PassesFilter(this float number, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||"))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(number, c)).Any(c => c);
            }
            if (filterString.Contains("&&"))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(number, c)).All(c => c);
            }
            if (filterString.StartsWith("=") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (number.ToString() == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("<=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult >= number)
                    {
                        return true;
                    }
                }
            }
            else if(filterString.StartsWith(">=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult <= number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("<") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult > number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith(">") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (double.TryParse(filter, out var numberResult))
                {
                    if (numberResult < number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("!") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                return !number.ToString().Contains(filter);
            }

            return number.ToString().Contains(filterString);
        }
        
        public static bool PassesFilter(this int number, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||"))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(number, c)).Any(c => c);
            }
            if (filterString.Contains("&&"))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(number, c)).All(c => c);
            }
            if (filterString.StartsWith("=") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (number.ToString() == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("<=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                var numberResult = 0;
                if (int.TryParse(filter, out numberResult))
                {
                    if (numberResult >= number)
                    {
                        return true;
                    }
                }
            }
            else if(filterString.StartsWith(">=") && filterString.Length >= 3)
            {
                var filter = filterString.Substring(2);
                var numberResult = 0;
                if (int.TryParse(filter, out numberResult))
                {
                    if (numberResult <= number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("<") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                var numberResult = 0;
                if (int.TryParse(filter, out numberResult))
                {
                    if (numberResult > number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith(">") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                var numberResult = 0;
                if (int.TryParse(filter, out numberResult))
                {
                    if (numberResult < number)
                    {
                        return true;
                    }
                }
            }
            else if (filterString.StartsWith("!") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                return !number.ToString().Contains(filter);
            }

            return number.ToString().Contains(filterString);
        }
    }
}