using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsCraftingItemFilter : BooleanFilter
{
    private readonly ExcelCache _excelCache;
    public override string Key { get; set; } = "IsCrafting";
    public override string Name { get; set; } = "Is Crafting Item?";
    public override string HelpText { get; set; } = "Only show items that relate to crafting.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        var currentValue = CurrentValue(configuration);
        return currentValue switch
        {
            null => null,
            true => _excelCache.IsCraftItem(item.Item.RowId),
            _ => !_excelCache.IsCraftItem(item.Item.RowId)
        };
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var currentValue = CurrentValue(configuration);

        return currentValue switch
        {
            null => null,
            true => _excelCache.IsCraftItem(item.RowId),
            _ => !_excelCache.IsCraftItem(item.RowId)
        };
    }

    public IsCraftingItemFilter(ILogger<IsCraftingItemFilter> logger, ImGuiService imGuiService,
        ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
}