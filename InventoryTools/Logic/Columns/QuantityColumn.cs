using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class QuantityColumn : IntegerColumn
    {
        private readonly IInventoryMonitor _inventoryMonitor;

        public QuantityColumn(ILogger<QuantityColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return (int)item.Quantity;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            var qty = 0;
            if (_inventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
            {
                qty += _inventoryMonitor.ItemCounts[(item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
            }
            if (_inventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)))
            {
                qty += _inventoryMonitor.ItemCounts[(item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)];
            }
            return qty;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Total Quantity Available";
        public override string RenderName => "Quantity";

        public override float Width { get; set; } = 70.0f;

        public override string HelpText { get; set; } =
            "The quantity of the item. If viewing from a game items or craft filter, this will show the total number of items available in all inventories.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        
        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter | Logic.FilterType.HistoryFilter;
    }
}