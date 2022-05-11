using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Misc;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class IsArmoireItem : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return item.Item == null ? false : CurrentValue(item.Item);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.IsArmoireItem(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Armoire?";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}