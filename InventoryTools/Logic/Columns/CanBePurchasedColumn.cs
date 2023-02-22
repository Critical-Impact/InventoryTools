using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class CanBePurchasedColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.Item.ObtainedGil;
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.ObtainedGil;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Can be Purchased?";
        public override float Width { get; set; } = 70.0f;
        public override string HelpText { get; set; } = "Can the item be purchased with gil?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}