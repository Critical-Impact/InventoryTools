using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class RarityColumn : TextColumn
    {
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            return item.FormattedRarity;
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Rarity";
        public override float Width { get; set; } = 70.0f;
        public override string HelpText { get; set; } = "The item's rarity.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}