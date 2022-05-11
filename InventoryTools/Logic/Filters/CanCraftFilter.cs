using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

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

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            if (item.Item == null)
            {
                return false;
            }
            return FilterItem(configuration, item.Item);
        }

        public override bool FilterItem(FilterConfiguration configuration,Item item)
        {
            var currentValue = CurrentValue(configuration);
            var canCraft = ExcelCache.CanCraftItem(item.RowId);
            return currentValue == null || currentValue.Value && canCraft || !currentValue.Value && !canCraft;
        }
    }
}