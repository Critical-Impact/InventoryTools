using System;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class PurchasedWithCurrencyColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return CurrentValue(item.Item);
        }

        public override string? CurrentValue(ItemEx item)
        {
            if (Service.ExcelCache.BoughtAtSpecialShop(item.RowId))
            {
                
                var currencyItems = Service.ExcelCache.GetCurrenciesByResultItemId(item.RowId);
                if (currencyItems != null)
                {
                    var names = currencyItems.Select(c =>
                    {
                        var items = Service.ExcelCache.GetSheet<ItemEx>();
                        return items.GetRow(c)?.Name ?? "Unknown";
                    }).Where(c => c != "").Distinct().ToList();
                    return String.Join(", ", names);
                }
            }

            return "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Purchased with";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}