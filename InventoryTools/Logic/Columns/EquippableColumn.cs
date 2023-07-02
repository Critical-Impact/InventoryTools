using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class EquippableColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return item.ClassJobCategory.Value?.Name ?? "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Equipped By (Class/Job)";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows what class/job an item can be equipped by";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}