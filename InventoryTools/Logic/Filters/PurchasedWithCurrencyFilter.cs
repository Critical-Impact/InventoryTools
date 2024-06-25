using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class PurchasedWithCurrencyFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "PurchaseWithCurrency";
        public override string Name { get; set; } = "Purchased with Currency";

        public override string HelpText { get; set; } =
            "Filter items based on the currency they can be purchased with.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override List<uint> DefaultValue { get; set; } = new();


        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration,item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return null;
            }
            
            return currentValue.Any(u => item.ObtainedWithSpecialShopCurrency(u) || item.ObtainedCompanyScrip && u is 20 or 21 or 22);
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            var currencies = _excelCache.GetCurrencies(3);
            currencies.Add(20);
            currencies.Add(21);
            currencies.Add(22);
            
            return currencies.ToDictionary(c => c, c => _excelCache.GetItemExSheet().GetRow(c)?.NameString ?? "Unknown").OrderBy(c => c.Value).ToDictionary(c => c.Key, c => c.Value);
        }

        public override bool HideAlreadyPicked { get; set; } = true;

        public PurchasedWithCurrencyFilter(ILogger<PurchasedWithCurrencyFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}