using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Stats
{
    public class ItemLevelFilter : StringFilter
    {
        public override string Key { get; set; } = "ILvl";
        public override string Name { get; set; } = "iLevel";
        public override string HelpText { get; set; } = "The iLevel of the item.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Stats;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (((int)item.Base.LevelItem.RowId).PassesFilter(currentValue.ToLower()))
                {
                    return true;
                }

                return false;
            }
            return true;
        }

        public ItemLevelFilter(ILogger<ItemLevelFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
            ShowOperatorTooltip = true;
        }
    }
}