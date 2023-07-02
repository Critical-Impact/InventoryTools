using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class IsAquariumItemColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.IsAquariumItem;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Aquarium Item?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Can this item be put in a aquarium?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Choice;
    }
}