using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class QuantityColumn : IntegerColumn
    {

        public override int? CurrentValue(InventoryItem item)
        {
            return (int)item.Quantity;
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

        public override string Name { get; set; } = "Quantity";
        public override float Width { get; set; } = 70.0f;

        public override string HelpText { get; set; } =
            "The quantity of the item. If viewing from a game items or craft filter, this will show the total number of items available in all inventories.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}