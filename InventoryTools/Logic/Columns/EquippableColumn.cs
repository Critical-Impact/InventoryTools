using CriticalCommonLib.Models;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class EquippableColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.Item != null) return CurrentValue(item.Item);
            return "";
        }

        public override string? CurrentValue(Item item)
        {
            return item.ClassJobCategory.Value?.Name ?? "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Equippable By";
        public override float Width { get; set; } = 200;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}