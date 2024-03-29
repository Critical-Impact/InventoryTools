﻿using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class RecipeTotalFilter : IntegerFilter
{
    public override string Key { get; set; } = "RecipeTotalFilter";
    public override string Name { get; set; } = "Recipe Total Count";
    public override string HelpText { get; set; } = "The number of recipes the item is a component of.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter |
                                                           FilterType.HistoryFilter | FilterType.GameItemFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }
        if (!Service.ExcelCache.ItemRecipeCount(item.RowId).PassesFilter(currentValue.Value.ToString()))
        {
            return false;
        }

        return true;
    }
}