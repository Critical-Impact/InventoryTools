using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class IsGearSetColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            if (item.GearSets == null)
            {
                return false;
            }
            return item.GearSets.Length != 0;
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return false;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "In Gearset?";
        public override float Width { get; set; } = 80;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}