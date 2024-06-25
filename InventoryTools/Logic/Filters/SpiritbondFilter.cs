using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class SpiritBondFilter : StringFilter
    {
        public override string Key { get; set; } = "SB";
        public override string Name { get; set; } = "Spirit Bond";
        public override string HelpText { get; set; } = "The spirit bond of the item.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (item.IsCollectible)
                {
                    return false;
                }
                var spiritBond = item.Spiritbond/100;
                if (!spiritBond.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public SpiritBondFilter(ILogger<SpiritBondFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
            ShowOperatorTooltip = true;
        }
    }
}