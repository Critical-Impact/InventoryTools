using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class LeveIsCraftLeveFilter : BooleanFilter
    {
        public LeveIsCraftLeveFilter(ILogger<LeveIsCraftLeveFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string Key { get; set; } = "LeveIsCraftLeve";
        public override string Name { get; set; } = "Is Craft Leve Item?";
        public override string HelpText { get; set; } = "Is this item craftable and a hand-in for a leve?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;



        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item.HasSourcesByType(ItemInfoType.CraftLeve),
                _ => !item.Item.HasSourcesByType(ItemInfoType.CraftLeve)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.HasSourcesByType(ItemInfoType.CraftLeve),
                _ => !item.HasSourcesByType(ItemInfoType.CraftLeve)
            };
        }
    }
}