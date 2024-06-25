using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsIshgardCraftFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsIshgardCraft";
        public override string Name { get; set; } = "Is Ishgardian Craft?";
        public override string HelpText { get; set; } = "Is this item a Ishgardian Restoration craft item?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsIshgardCraft)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsIshgardCraft;
        }

        public IsIshgardCraftFilter(ILogger<IsIshgardCraftFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}