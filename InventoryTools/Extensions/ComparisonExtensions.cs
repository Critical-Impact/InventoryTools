using System.Globalization;

namespace InventoryTools.Extensions
{
    public static class ComparisonExtensions
    {
        public static bool PassesFilter(this string text, string filterString)
        {
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