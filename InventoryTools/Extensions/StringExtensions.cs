using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace InventoryTools.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static string ToSentence( this string input )
        {
            return new string(input.SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new[] { ' ', c } : new[] { c }).ToArray());
        }
    }
}