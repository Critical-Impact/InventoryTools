using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CanBeDyedColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.IsDyeable;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Dyeable?";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "Can the item be dyed?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}