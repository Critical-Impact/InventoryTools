using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsGearSetFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsGearSet";
        public override string Name { get; set; } = "Is Part of Gearset?";
        public override string HelpText { get; set; } = "Is the item a part of a gearset?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }

            if (item.GearSets == null)
            {
                return !currentValue.Value;
            }
            switch (currentValue.Value)
            {
                case false:
                    return item.GearSets.Length == 0;
                case true:
                    return item.GearSets.Length != 0;
            }
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public IsGearSetFilter(ILogger<IsGearSetFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}