using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class IsIshgardCraftColumn : CheckboxColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.Item == null ? false : CurrentValue(item.Item);
        }

        public override bool? CurrentValue(ItemEx item)
        {
            return item.IsIshgardCraft;
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Ishgardian Craft?";
        public override float Width { get; set; } = 100;
        public override string HelpText { get; set; } = "Is this item a Ishgardian Restoration craft item?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}