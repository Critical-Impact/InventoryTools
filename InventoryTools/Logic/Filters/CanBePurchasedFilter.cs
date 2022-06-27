using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class CanBePurchasedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "Purchasable";
        public override string Name { get; set; } = "Can be Purchased?";
        public override string HelpText { get; set; } = "Can this be item be purchased?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            if (item.Item == null)
            {
                return false;
            }
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration,Item item)
        {
            var currentValue = CurrentValue(configuration);
            var canBePurchased = ExcelCache.IsItemGilShopBuyable(item.RowId);
            return currentValue == null || currentValue.Value && canBePurchased || !currentValue.Value && !canBePurchased;
        }
    }
}