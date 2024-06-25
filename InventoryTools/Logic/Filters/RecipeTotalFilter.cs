using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class RecipeTotalFilter : StringFilter
{
    private readonly ExcelCache _excelCache;
    public override string Key { get; set; } = "RecipeTotalFilter";
    public override string Name { get; set; } = "Recipe Total Count";
    public override string HelpText { get; set; } = "The number of recipes the item is a component of.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

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
        if (!_excelCache.ItemRecipeCount(item.RowId).PassesFilter(currentValue))
        {
            return false;
        }

        return true;
    }

    public RecipeTotalFilter(ILogger<RecipeTotalFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
        ShowOperatorTooltip = true;
    }
}