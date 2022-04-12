using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class RarityColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            if (item.Item == null)
            {
                return null;
            }
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(Item item)
        {
            return item.FormattedRarity();
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Rarity";
        public override float Width { get; set; } = 70.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        
        public override event IColumn.ButtonPressedDelegate? ButtonPressed;
    }
}