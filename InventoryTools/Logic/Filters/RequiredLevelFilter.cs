using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class RequiredLevelFilter : StringFilter
    {
        public override string Key { get; set; } = "ItemLvl";
        public override string Name { get; set; } = "Required Level";
        public override string HelpText { get; set; } = "The required level to equip the item.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (((int)item.Base.LevelEquip).PassesFilter(currentValue.ToLower()))
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        public RequiredLevelFilter(ILogger<RequiredLevelFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
            ShowOperatorTooltip = true;
        }
    }
}