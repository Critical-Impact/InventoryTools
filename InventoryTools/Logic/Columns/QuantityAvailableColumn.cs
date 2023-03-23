using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class QuantityAvailableColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override int? CurrentValue(ItemEx item)
        {
            return (int?) PluginService.InventoryMonitor.ItemCounts.Where(c => c.Key.Item1 == item.RowId).Sum(c => c.Value);
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Available";
        public override float Width { get; set; } = 100;

        public override string HelpText { get; set; } =
            "The number of items available across all inventories of this item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}