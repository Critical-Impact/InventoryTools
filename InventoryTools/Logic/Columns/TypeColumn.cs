using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class TypeColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override string? CurrentValue(InventoryItem item)
        {
            return item.FormattedType;
        }

        public override string? CurrentValue(ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Type";
        public override float Width { get; set; } = 80.0f;
        public override string HelpText { get; set; } = "The type of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}