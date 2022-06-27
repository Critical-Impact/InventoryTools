using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class CraftItemsFilter : StringFilter
    {
        public override string Key { get; set; } = "CraftItemsFilter";
        public override string Name { get; set; } = "Items Required to Craft";

        public override string HelpText { get; set; } =
            "Enter the name of an item and the list of items will be filtered down to just contain these.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;

        public Item? foundItem = null;

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, string newValue)
        {
            foundItem = null;
            base.UpdateFilterConfiguration(configuration, newValue);
        }

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            if (item.Item != null)
            {
                return FilterItem(configuration, item.Item);
            }

            return true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue != "")
            {
                if (foundItem == null)
                {
                    var excelSheet = ExcelCache.GetSheet<Item>();
                    var matchesItem = excelSheet.Any(c => c.Name.ToString() == currentValue);
                    if (matchesItem)
                    {
                        foundItem = excelSheet.Single(c => c.Name.ToString() == currentValue);
                    }
                }

                if (foundItem != null)
                {
                    var recipes = ExcelCache.GetItemRecipes(foundItem.RowId);
                    foreach (var recipe in recipes)
                    {
                        foreach (var ingredient in recipe.UnkData5)
                        {
                            if (ingredient.ItemIngredient != 0)
                            {
                                if (item.RowId == (uint) ingredient.ItemIngredient)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            return true;
        }
    }
}