using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

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
                true => item.Item.Base.AdditionalData.RowId != 0 && item.Item.Base.ItemAction.RowId == 0 && item.Item.Base.FilterGroup != 37 && item.Item.Base.FilterGroup != 15 && item.Item.Base.FilterGroup != 39 && item.Item.Base.FilterGroup != 18,
                _ => !(item.Item.Base.AdditionalData.RowId != 0 && item.Item.Base.ItemAction.RowId == 0 && item.Item.Base.FilterGroup != 37 && item.Item.Base.FilterGroup != 15 && item.Item.Base.FilterGroup != 39 && item.Item.Base.FilterGroup != 18)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Base.AdditionalData.RowId != 0 && item.Base.ItemAction.RowId == 0 && item.Base.FilterGroup != 37 && item.Base.FilterGroup != 15 && item.Base.FilterGroup != 39 && item.Base.FilterGroup != 18,
                _ => !(item.Base.AdditionalData.RowId != 0 && item.Base.ItemAction.RowId == 0 && item.Base.FilterGroup != 37 && item.Base.FilterGroup != 15 && item.Base.FilterGroup != 39 && item.Base.FilterGroup != 18)
            };
        }

        public IsHousingItemFilter(ILogger<IsHousingItemFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}