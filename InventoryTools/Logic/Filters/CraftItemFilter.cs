using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CraftItemFilter : IntegerFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "CraftItemFilter";
        public override string Name { get; set; } = "WIP: Craft Item Filter";

        public override string HelpText { get; set; } =
            "Enter the ID of an item to list only items applicable to it's craft";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;


        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return true;
            }

            var expectedItem = _excelCache.GetItemExSheet().GetRow((uint) currentValue.Value);
            if (expectedItem != null)
            {
                return true;
            }

            if (_excelCache.CanCraftItem((uint)currentValue.Value))
            {
                var flattenedRecipe = _excelCache.GetFlattenedItemRecipe((uint) currentValue.Value);
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

            var excelItem = _excelCache.GetItemExSheet().GetRow((uint) currentValue.Value);
            if (excelItem == null)
            {
                return true;
            }

            if (_excelCache.CanCraftItem((uint)currentValue.Value))
            {
                var flattenedRecipe = _excelCache.GetFlattenedItemRecipe((uint) currentValue.Value, true);
                return flattenedRecipe.Any(c => c.Key == item.RowId);
            }

            return false;
        }

        public CraftItemFilter(ILogger<CraftItemFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}