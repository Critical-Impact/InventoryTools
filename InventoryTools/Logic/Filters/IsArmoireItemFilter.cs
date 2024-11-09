using AllaganLib.GameSheets.Sheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsArmoireItemFilter : BooleanFilter
    {
        public IsArmoireItemFilter(ILogger<IsArmoireItemFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override string Key { get; set; } = "IsArmoire";
        public override string Name { get; set; } = "Is Armoire Item?";
        public override string HelpText { get; set; } = "Only show items that can be put into the armoire.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;




        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item.HasUsesByType(ItemInfoType.Armoire),
                _ => !item.Item.HasUsesByType(ItemInfoType.Armoire)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.HasUsesByType(ItemInfoType.Armoire),
                _ => !item.HasUsesByType(ItemInfoType.Armoire)
            };
        }
    }
}