using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class HighlightWhenFilter : ChoiceFilter<string>
    {
        public readonly string[] HighlightWhenItemsFilter = new string[] {"N/A", "Always", "When Searching"};
        public override string? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.HighlightWhen ?? DefaultValue;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, string? newValue)
        {
            configuration.HighlightWhen = newValue != null && newValue != DefaultValue ? newValue : null;
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        public override string? DefaultValue { get; set; } = "N/A";


        public override string Key { get; set; } = "HighlightWhen";
        public override string Name { get; set; } = "Highlight When?";
        public override string HelpText { get; set; } = "When should the highlighting apply?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

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

        public override string GetFormattedChoice(FilterConfiguration filterConfiguration, string choice)
        {
            return choice;
        }

        public HighlightWhenFilter(ILogger<HighlightWhenFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}