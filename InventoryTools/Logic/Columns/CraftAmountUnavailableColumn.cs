using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountUnavailableColumn : IntegerColumn
    {
        public override int? CurrentValue(InventoryItem item)
        {
            return 0;
        }

        public override int? CurrentValue(Item item)
        {
            return 0;
        }

        public override int? CurrentValue(SortingResult item)
        {
            return 0;
        }

        public override int? CurrentValue(CraftItem currentValue)
        {
            return (int)currentValue.QuantityUnavailable;
        }
        
        public override string Name { get; set; } = "Unavailable";
        public override float Width { get; set; } = 60;
        public override string FilterText { get; set; } = "This is the amount that needs to be sourced from MB/gathering.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}