using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class NpcHighlightFilter : ChoiceFilter<NpcHighlight>
    {
        public override NpcHighlight CurrentValue(FilterConfiguration configuration)
        {
            return configuration.NpcHighlightEnum;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, NpcHighlight newValue)
        {
            configuration.NpcHighlightEnum = newValue;
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        public override NpcHighlight DefaultValue { get; set; } = NpcHighlight.UseGlobalConfiguration;

        public override string Key { get; set; } = "NpcHighlight";
        public override string Name { get; set; } = "Highlight NPCs?";
        public override string HelpText { get; set; } = "Should items required by this list highlight NPCs that sell them?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override List<NpcHighlight> GetChoices(FilterConfiguration configuration)
        {
            return [NpcHighlight.UseGlobalConfiguration, NpcHighlight.Yes, NpcHighlight.No];
        }

        public override string GetFormattedChoice(FilterConfiguration filterConfiguration, NpcHighlight choice)
        {
            switch (choice)
            {
                case NpcHighlight.UseGlobalConfiguration:
                    return "Use Global Configuration";
                case NpcHighlight.Yes:
                    return "Yes";
                case NpcHighlight.No:
                    return "No";
            }

            return choice.ToString();
        }

        public NpcHighlightFilter(ILogger<NpcHighlightFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
