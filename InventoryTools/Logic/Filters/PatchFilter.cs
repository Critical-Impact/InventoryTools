using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class PatchFilter : StringFilter
{
    public override string Key { get; set; } = "PatchFilter";
    public override string Name { get; set; } = "Patch";
    public override string HelpText { get; set; } = "The patch in which the item was added.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter | FilterType.SearchFilter |
                                                           FilterType.SortingFilter | FilterType.GameItemFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);
        if (!string.IsNullOrEmpty(currentValue))
        {
            if (item.GetPatch().PassesFilter(currentValue.ToLower()))
            {
                return true;
            }

            return false;
        }
        return true;
    }
}