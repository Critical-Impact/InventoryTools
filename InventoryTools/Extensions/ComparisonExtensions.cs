using System.Linq;

namespace InventoryTools.Extensions
{
    public static class ComparisonExtensions
    {
        public static bool PassesFilter(this string text, string filterString)
        {
            filterString = filterString.Trim();
            if (filterString.Contains("||"))
            {
                var ors = filterString.Split("||");
                return ors.Select(c => PassesFilter(text, c)).Any(c => c);
            }
            if (filterString.Contains("&&"))
            {
                var ands = filterString.Split("&&");
                return ands.Select(c => PassesFilter(text, c)).All(c => c);
            }
            if (filterString.StartsWith("=") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                if (text == filter)
                {
                    return true;
                }
            }
            else if (filterString.StartsWith("!") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1);
                return !text.Contains(filter);
            }
            else if (filterString.StartsWith("~") && filterString.Length >= 2)
            {
                var filter = filterString.Substring(1).Split(" ");
                var splitText = text.Split(" ");
                return filter.All(c => splitText.Any(d => d.Contains(c)));
            }

            return text.Contains(filterString);
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