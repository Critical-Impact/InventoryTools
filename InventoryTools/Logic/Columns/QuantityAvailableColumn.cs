using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class QuantityAvailableColumn : IntegerColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override int? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override int? CurrentValue(ItemEx item)
        {
            var qty = 0;
            if (PluginService.InventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)))
            {
                qty += PluginService.InventoryMonitor.ItemCounts[(item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None)];
            }
            if (PluginService.InventoryMonitor.ItemCounts.ContainsKey((item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)))
            {
                qty += PluginService.InventoryMonitor.ItemCounts[(item.RowId,
                    FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ)];
            }
            return qty;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
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