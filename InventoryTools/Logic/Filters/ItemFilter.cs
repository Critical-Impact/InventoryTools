using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class ItemFilter : UintMultipleChoiceFilter
{
    public override string Key { get; set; } = "ItemFilter";
    public override string Name { get; set; } = "Name (Selector)";

    public override string HelpText { get; set; } =
        "Select a list of items and the filter will only display these items.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override List<uint> DefaultValue { get; set; } = new();

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.HistoryFilter |
                                                           FilterType.SortingFilter | FilterType.GameItemFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var searchItems = CurrentValue(configuration).ToList();
        if (searchItems.Count == 0)
        {
            return null;
        }

        if (searchItems.Contains(item.RowId))
        {
            return true;
        }

        return false;
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        return Service.ExcelCache.ItemNamesById;
    }

    public override bool HideAlreadyPicked { get; set; }
}