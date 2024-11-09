using InventoryTools.Logic;

namespace InventoryTools.Extensions;

public static class FilterTypeExtensions
{
    public static string FormattedName(this FilterType filterType)
    {
        return filterType switch
        {
            FilterType.None => "None",
            FilterType.SearchFilter => "Search List",
            FilterType.SortingFilter => "Sort List",
            FilterType.GameItemFilter => "Game Item List",
            FilterType.CraftFilter => "Craft List",
            FilterType.HistoryFilter => "History List",
            FilterType.CuratedList => "Curated List",
            _ => "Unknown"
        };
    }
}