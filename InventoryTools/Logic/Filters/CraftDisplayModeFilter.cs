using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftDisplayModeFilter : ChoiceFilter<CraftDisplayMode>
{
    public override CraftDisplayMode CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftDisplayMode;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftDisplayMode = DefaultValue;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftDisplayMode newValue)
    {
        configuration.CraftDisplayMode = newValue;
    }

    public override string Key { get; set; } = "CraftDisplayMode";
    public override string Name { get; set; } = "Craft Display Mode";

    public override string HelpText { get; set; } =
        "Should the craft items be placed in a single table or grouped into multiple tabs.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
    public override CraftDisplayMode DefaultValue { get; set; } = CraftDisplayMode.SingleTable;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override List<CraftDisplayMode> GetChoices(FilterConfiguration configuration)
    {
        return new List<CraftDisplayMode>()
        {
            CraftDisplayMode.SingleTable,
            CraftDisplayMode.Tabs
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftDisplayMode choice)
    {
        switch (choice)
        {
            case CraftDisplayMode.Tabs:
                return "Tabs";
            case CraftDisplayMode.SingleTable:
                return "Single Table";
        }

        return choice.ToString();
    }

    public CraftDisplayModeFilter(ILogger<CraftDisplayModeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}