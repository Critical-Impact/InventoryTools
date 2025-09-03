using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class HighlightWhenFilter : ChoiceFilter<HighlightWhen>
    {
        public override HighlightWhen CurrentValue(FilterConfiguration configuration)
        {
            return configuration.HighlightWhenEnum;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, HighlightWhen newValue)
        {
            configuration.HighlightWhenEnum = newValue;
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        public override HighlightWhen DefaultValue { get; set; } = HighlightWhen.UseGlobalConfiguration;


        public override string Key { get; set; } = "HighlightWhen";
        public override string Name { get; set; } = "Highlight When?";
        public override string HelpText { get; set; } = "When should the highlighting apply?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override List<HighlightWhen> GetChoices(FilterConfiguration configuration)
        {
            return [HighlightWhen.UseGlobalConfiguration, HighlightWhen.WhenSearching, HighlightWhen.Always];
        }

        public override string GetFormattedChoice(FilterConfiguration filterConfiguration, HighlightWhen choice)
        {
            switch (choice)
            {
                case HighlightWhen.UseGlobalConfiguration:
                    return "Use Global Configuration";
                case HighlightWhen.WhenSearching:
                    return "When Searching";
                case HighlightWhen.Always:
                    return "Always";
            }

            return choice.ToString();
        }

        public HighlightWhenFilter(ILogger<HighlightWhenFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}