using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CraftAmountRequiredColumn : IntegerColumn
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
            return (int)currentValue.QuantityRequired;
        }
        
        public override string Name { get; set; } = "Amount Required";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "This is the amount required to complete the craft.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}