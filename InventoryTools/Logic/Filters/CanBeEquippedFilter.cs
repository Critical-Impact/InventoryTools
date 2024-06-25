using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CanBeEquippedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanBeEquipped";
        public override string Name { get; set; } = "Can be Equipped?";
        public override string HelpText { get; set; } = "Can the item be equipped?";
        
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
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

            if (currentValue.Value && item.EquipSlotCategory.Row != 0)
            {
                return true;
            }

            return false;
        }

        public CanBeEquippedFilter(ILogger<CanBeEquippedFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}