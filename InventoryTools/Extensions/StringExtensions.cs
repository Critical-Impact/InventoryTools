using System.Diagnostics;
using System.Globalization;

namespace InventoryTools.Extensions
{
    public static class StringExtensions
    {
        public static void OpenBrowser(this string url) {
            Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }
        
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()); 
        }
    }
}