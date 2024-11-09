using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class NameFilter : StringFilter
    {
        public override string Key { get; set; } = "Name";
        public override string Name { get; set; } = "Name";
        public override string HelpText { get; set; } = "Searches by the name of the item.";

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
                if (!item.NameString.ToString().ToLower().PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public NameFilter(ILogger<NameFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}