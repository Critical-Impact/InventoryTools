using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class HighlightWhenFilter : ChoiceFilter<string>
    {
        public readonly string[] HighlightWhenItemsFilter = new string[] {"N/A", "Always", "When Searching"};
        public override string? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.HighlightWhen ?? EmptyValue;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, string? newValue)
        {
            configuration.HighlightWhen = newValue != null && newValue != EmptyValue ? newValue : null;
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, EmptyValue);
        }

        public override string EmptyValue { get; set; } = "N/A";


        public override string Key { get; set; } = "HighlightWhen";
        public override string Name { get; set; } = "Highlight When?";
        public override string HelpText { get; set; } = "When should the highlighting apply?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.CraftFilter | FilterType.HistoryFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public override List<string> GetChoices(FilterConfiguration configuration)
        {
            return HighlightWhenItemsFilter.ToList();
        }

        public override string GetFormattedChoice(string choice)
        {
            return choice;
        }
    }
}