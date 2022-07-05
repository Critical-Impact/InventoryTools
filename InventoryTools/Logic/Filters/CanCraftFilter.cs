using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CanCraftFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanCraft";
        public override string Name { get; set; } = "Can Craft?";
        public override string HelpText { get; set; } = "Can this be crafted?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canCraft = Service.ExcelCache.CanCraftItem(item.RowId);
            return currentValue == null || currentValue.Value && canCraft || !currentValue.Value && !canCraft;
        }
    }
}