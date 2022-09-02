using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class CraftItemFilter : IntegerFilter
    {
        public override string Key { get; set; } = "CraftItemFilter";
        public override string Name { get; set; } = "WIP: Craft Item Filter";

        public override string HelpText { get; set; } =
            "Enter the ID of an item to list only items applicable to it's craft";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return true;
            }

            var expectedItem = Service.ExcelCache.GetItemExSheet().GetRow((uint) currentValue.Value);
            if (expectedItem != null)
            {
                return true;
            }

            if (Service.ExcelCache.CanCraftItem((uint)currentValue.Value))
            {
                var flattenedRecipe = Service.ExcelCache.GetFlattenedItemRecipe((uint) currentValue.Value);
                return flattenedRecipe.Any(c => c.Key == item.ItemId);
            }
            return false;
        }


        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return true;
            }

            var excelItem = Service.ExcelCache.GetItemExSheet().GetRow((uint) currentValue.Value);
            if (excelItem == null)
            {
                return true;
            }

            if (Service.ExcelCache.CanCraftItem((uint)currentValue.Value))
            {
                var flattenedRecipe = Service.ExcelCache.GetFlattenedItemRecipe((uint) currentValue.Value, true);
                return flattenedRecipe.Any(c => c.Key == item.RowId);
            }

            return false;
        }
    }
}