using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class QuantityFilter : StringFilter
    {
        private readonly IInventoryMonitor _inventoryMonitor;

        public QuantityFilter(ILogger<QuantityFilter> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
            ShowOperatorTooltip = true;
        }
        public override string Key { get; set; } = "Qty";
        public override string Name { get; set; } = "Quantity";
        public override string HelpText { get; set; } = "The quantity of the item.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (!item.Quantity.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                var qty = 0;
                if (_inventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
                {
                    qty += _inventoryMonitor.ItemCounts[(item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
                }
                if (_inventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)))
                {
                    qty += _inventoryMonitor.ItemCounts[(item.RowId,
                        FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)];
                }
                if (!qty.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}