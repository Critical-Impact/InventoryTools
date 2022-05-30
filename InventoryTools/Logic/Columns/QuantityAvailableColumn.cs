using System.Linq;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class QuantityAvailableColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            if (item.Item == null)
            {
                return null;
            }
            return CurrentValue(item.Item);
        }

        public override int? CurrentValue(Item item)
        {
            return (int?) PluginService.InventoryMonitor.AllItems.Where(c => c.ItemId == item.RowId).Sum(c => c.Quantity);
        }

        public override int? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Quantity Available";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}