using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class HighlightWhenFilter : ChoiceFilter<string>
    {
        public readonly string[] HighlightWhenItemsFilter = new string[] {"N/A", "Always", "When Searching"};
        public override KeyValuePair<string, string>? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.HighlightWhen != null ? new KeyValuePair<string, string>(configuration.HighlightWhen, configuration.HighlightWhen) : new KeyValuePair<string, string>(EmptyValue,EmptyValue);
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, KeyValuePair<string, string>? newValue)
        {
            configuration.HighlightWhen = newValue != null && newValue.Value.Key != EmptyValue ? newValue.Value.Key : null;
        }

        public override string EmptyValue { get; set; } = "N/A";


        public override string Key { get; set; } = "HighlightWhen";
        public override string Name { get; set; } = "Highlight When?";
        public override string HelpText { get; set; } = "When should the highlighting apply?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override Dictionary<string, string> GetChoices(FilterConfiguration configuration)
        {
            return HighlightWhenItemsFilter.ToDictionary(c => c, c => c);
        }
    }
}