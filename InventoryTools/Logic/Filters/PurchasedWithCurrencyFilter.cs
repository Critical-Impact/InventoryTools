using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class PurchasedWithCurrencyFilter : UintMultipleChoiceFilter
    {
        public override string Key { get; set; } = "PurchaseWithCurrency";
        public override string Name { get; set; } = "Purchased with Currency";

        public override string HelpText { get; set; } =
            "Filter items based on the currency they can be purchased with.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return null;
            }
            return currentValue.Any(currencyItem => ExcelCache.BoughtWithCurrency(currencyItem, item.ItemId));
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return null;
            }
            return currentValue.Any(currencyItem => ExcelCache.BoughtWithCurrency(currencyItem, item.RowId));
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            var currencies = ExcelCache.GetCurrencies(3);
            return currencies.ToDictionary(c => c, c => ExcelCache.GetItem(c)?.Name ?? "Unknown").OrderBy(c => c.Value).ToDictionary(c => c.Key, c => c.Value);
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}