using System.Web;
using AllaganLib.GameSheets.Sheets.Rows;

namespace InventoryTools.Extensions;

public static class ItemRowExtensions
{
    public static string ToGarlandToolsUrl(this ItemRow itemRow)
    {
        return $"https://www.garlandtools.org/db/#item/{itemRow.GarlandToolsId}";
    }

    public static string ToTeamCraftUrl(this ItemRow itemRow)
    {
        return $"https://ffxivteamcraft.com/db/en/item/{itemRow.RowId}";
    }

    public static string ToUniversalisUrl(this ItemRow itemRow)
    {
        return $"https://universalis.app/market/{itemRow.RowId}";
    }

    public static string ToGamerEscapeUrl(this ItemRow itemRow)
    {
        var name = itemRow.NameString.Replace(' ', '_');
        name = name.Replace('–', '-');

        if (name.StartsWith("_")) // "level sync" icon
            name = name.Substring(2);
        return $"https://ffxiv.gamerescape.com/wiki/{HttpUtility.UrlEncode(name)}?useskin=Vector";
    }

    public static string ToConsoleGamesWikiUrl(this ItemRow itemRow)
    {
        var name = itemRow.NameString.Replace("#"," ").Replace("  ", " ").Replace(' ', '_');
        name = name.Replace('–', '-');

        if (name.StartsWith("_")) // "level sync" icon
            name = name.Substring(2);
        return $"https://ffxiv.consolegameswiki.com/wiki/{HttpUtility.UrlEncode(name)}";
    }
}