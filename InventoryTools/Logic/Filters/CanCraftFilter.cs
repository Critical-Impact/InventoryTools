using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CanCraftFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanCraft";
        public override string Name { get; set; } = "Can Craft?";
        public override string HelpText { get; set; } = "Can this be crafted?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

        public CanCraftFilter(ILogger<CanCraftFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            var canCraft = item.HasSourcesByType(ItemInfoType.CraftRecipe);
            return currentValue == null || currentValue.Value && canCraft || !currentValue.Value && !canCraft;
        }
    }
}