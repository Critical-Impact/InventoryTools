using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsCurrencyFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsCurrency";
        public override string Name { get; set; } = "Is Currency?";
        public override string HelpText { get; set; } = "Is this traded for items as specific shops?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;


        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
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
                    return !item.SpentSpecialShop;
                case true:
                    return item.SpentSpecialShop;
            }
        }

        public IsCurrencyFilter(ILogger<IsCurrencyFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}