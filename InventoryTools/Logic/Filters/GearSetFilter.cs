using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class GearSetFilter : StringFilter
    {
        public override string Key { get; set; } = "GearSet";
        public override string Name { get; set; } = "Gear Sets";
        public override string HelpText { get; set; } = "Filter by the gear sets that a item is in.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (item.GearSetNames != null)
                {
                    var gearSetNames = String.Join(", ", item.GearSetNames);
                    if (!gearSetNames.ToLower().PassesFilter(currentValue.ToLower()))
                    {
                        return false;
                    }
                }
            }

            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public GearSetFilter(ILogger<GearSetFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}