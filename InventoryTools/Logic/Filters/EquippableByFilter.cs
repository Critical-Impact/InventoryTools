using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByFilter : UintMultipleChoiceFilter
    {
        public override string Key { get; set; } = "EquippableBy";
        public override string Name { get; set; } = "Equippable By";
        public override string HelpText { get; set; } = "Which classes can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return item.Item != null && FilterItem(configuration, item.Item);
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = this.CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return true;
            }
            ExcelCache.CalculateClassJobCategoryLookup();
            var lookup = ExcelCache.ClassJobCategoryLookup;
            if (lookup.ContainsKey(item.ClassJobCategory.Row))
            {
                var map = lookup[item.ClassJobCategory.Row];
                if (map.Any(c => currentValue.Contains(c)))
                {
                    return true;
                }
            }

            return false;
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            var choices = new Dictionary<uint, string>();
            var sheet = ExcelCache.GetSheet<ClassJob>();
            foreach (var classJob in sheet)
            {
                choices.Add(classJob.RowId, classJob.Name.ToString().ToTitleCase());
            }

            return choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}