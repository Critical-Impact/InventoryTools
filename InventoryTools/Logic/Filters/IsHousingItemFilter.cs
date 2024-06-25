using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsHousingItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsHousing";
        public override string Name { get; set; } = "Is Housing Item?";
        public override string HelpText { get; set; } = "Only show items that relate to housing.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item.AdditionalData != 0 && item.Item.ItemAction.Row == 0 && item.Item.FilterGroup != 37 && item.Item.FilterGroup != 15 && item.Item.FilterGroup != 39 && item.Item.FilterGroup != 18,
                _ => !(item.Item.AdditionalData != 0 && item.Item.ItemAction.Row == 0 && item.Item.FilterGroup != 37 && item.Item.FilterGroup != 15 && item.Item.FilterGroup != 39 && item.Item.FilterGroup != 18)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.AdditionalData != 0 && item.ItemAction.Row == 0 && item.FilterGroup != 37 && item.FilterGroup != 15 && item.FilterGroup != 39 && item.FilterGroup != 18,
                _ => !(item.AdditionalData != 0 && item.ItemAction.Row == 0 && item.FilterGroup != 37 && item.FilterGroup != 15 && item.FilterGroup != 39 && item.FilterGroup != 18)
            };
        }

        public IsHousingItemFilter(ILogger<IsHousingItemFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}