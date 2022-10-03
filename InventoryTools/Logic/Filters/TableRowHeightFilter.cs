using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class TableRowHeightFilter : IntegerFilter
{
    public override string Key { get; set; } = "TableRowHeight";
    public override string Name { get; set; } = "Table Row Height";
    public override string HelpText { get; set; } = "How many pixels high should each item row try to display at?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

    public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter |
                                                           FilterType.GameItemFilter | FilterType.CraftFilter;

    public override int? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.TableHeight;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, int? newValue)
    {
        if (newValue != null)
        {
            configuration.TableHeight = Math.Clamp(newValue.Value, 8, 128);
        }
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        return null;
    }
}