using CriticalCommonLib.Services;

using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class QuantityAvailableColumn : IntegerColumn
    {
        private readonly IInventoryMonitor _inventoryMonitor;

        public QuantityAvailableColumn(ILogger<QuantityAvailableColumn> logger, ImGuiService imGuiService, IInventoryMonitor inventoryMonitor) : base(logger, imGuiService)
        {
            _inventoryMonitor = inventoryMonitor;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var qty = 0;
            if (_inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
            }
            if (_inventoryMonitor.ItemCounts.ContainsKey((searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)))
            {
                qty += _inventoryMonitor.ItemCounts[(searchResult.Item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality)];
            }
            return qty;
        }
        public override string Name { get; set; } = "Total Quantity Available";
        public override string RenderName => "Available";
        public override float Width { get; set; } = 100;

        public override string HelpText { get; set; } =
            "The number of items available across all inventories of this item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType AvailableIn => Logic.FilterType.CraftFilter;
    }
}