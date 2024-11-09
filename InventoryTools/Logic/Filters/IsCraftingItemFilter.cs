using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsCraftingItemFilter : BooleanFilter
{
    public IsCraftingItemFilter(ILogger<IsCraftingItemFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

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
            true => item.Item.HasUsesByType(ItemInfoType.CraftRecipe, ItemInfoType.FreeCompanyCraftRecipe),
            _ => !item.Item.HasUsesByType(ItemInfoType.CraftRecipe, ItemInfoType.FreeCompanyCraftRecipe)
        };
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);

        return currentValue switch
        {
            null => null,
            true => item.HasUsesByType(ItemInfoType.CraftRecipe, ItemInfoType.FreeCompanyCraftRecipe),
            _ => !item.HasUsesByType(ItemInfoType.CraftRecipe, ItemInfoType.FreeCompanyCraftRecipe)
        };
    }
}