using System.Diagnostics;

namespace InventoryTools.Extensions
{
    public static class StringExtensions
    {
        public static void OpenBrowser(this string url) {
            Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }
    }
}