using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class IsCurrencyFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsCurrency";
        public override string Name { get; set; } = "Is Currency?";
        public override string HelpText { get; set; } = "Is this traded for items as specific shops?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }

            switch (currentValue.Value)
            {
                case false:
                    return !Service.ExcelCache.SpentAtSpecialShop(item.ItemId);
                case true:
                    return Service.ExcelCache.SpentAtSpecialShop(item.ItemId);
            }
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }

            switch (currentValue.Value)
            {
                case false:
                    return !Service.ExcelCache.SpentAtSpecialShop(item.RowId);
                case true:
                    return Service.ExcelCache.SpentAtSpecialShop(item.RowId);
            }
        }
    }
}