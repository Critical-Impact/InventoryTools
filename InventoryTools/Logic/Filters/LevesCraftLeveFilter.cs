using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class LeveIsCraftLeveFilter : BooleanFilter
    {
        public override string Key { get; set; } = "LeveIsCraftLeve";
        public override string Name { get; set; } = "Is Craft Leve Item?";
        public override string HelpText { get; set; } = "Is this item craftable and a hand-in for a leve?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => item.Item != null && ExcelCache.IsItemCraftLeve(item.ItemId),
                _ => item.Item != null && !ExcelCache.IsItemCraftLeve(item.ItemId)
            };
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => ExcelCache.IsItemCraftLeve(item.RowId),
                _ => !ExcelCache.IsItemCraftLeve(item.RowId)
            };
        }
    }
}