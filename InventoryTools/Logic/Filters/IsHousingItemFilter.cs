using System.Collections.Generic;
using CriticalCommonLib.Models;
using Dalamud.Logging;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Misc;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class IsHousingItemFilter : BooleanFilter
    {
        public override string Key { get; set; } = "IsHousing";
        public override string Name { get; set; } = "Is Housing Item?";
        public override string HelpText { get; set; } = "Only show items that relate to housing.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item != null && item.Item.AdditionalData != 0 && item.Item.ItemAction.Row == 0 && item.Item.FilterGroup != 37 && item.Item.FilterGroup != 15 && item.Item.FilterGroup != 39 && item.Item.FilterGroup != 18,
                _ => item.Item != null && !(item.Item.AdditionalData != 0 && item.Item.ItemAction.Row == 0 && item.Item.FilterGroup != 37 && item.Item.FilterGroup != 15 && item.Item.FilterGroup != 39 && item.Item.FilterGroup != 18)
            };
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.AdditionalData != 0 && item.ItemAction.Row == 0 && item.FilterGroup != 37 && item.FilterGroup != 15 && item.FilterGroup != 39 && item.FilterGroup != 18,
                _ => !(item.AdditionalData != 0 && item.ItemAction.Row == 0 && item.FilterGroup != 37 && item.FilterGroup != 15 && item.FilterGroup != 39 && item.FilterGroup != 18)
            };
        }
    }
}