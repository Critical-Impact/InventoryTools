using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class CanBePurchasedColumn : CheckboxColumn
    {
        public override bool? CurrentValue(InventoryItem item)
        {
            return ExcelCache.IsItemGilShopBuyable(item.ItemId);
        }

        public override bool? CurrentValue(Item item)
        {
            return ExcelCache.IsItemGilShopBuyable(item.RowId);
        }

        public override bool? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Can be Purchased?";
        public override float Width { get; set; } = 125.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}