using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftOutputOrderingFilter : ChoiceFilter<OutputOrderingSetting>
{
    public override OutputOrderingSetting CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.OutputOrderingSetting;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.OutputOrderingSetting = DefaultValue;
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, OutputOrderingSetting newValue)
    {
        configuration.CraftList.OutputOrderingSetting = newValue;
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftOutputOrderingFilter";
    public override string Name { get; set; } = "Output Ordering";

    public override string HelpText { get; set; } =
        "Should the list of output items be ordered in a specific way?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override OutputOrderingSetting DefaultValue { get; set; } = OutputOrderingSetting.AsAdded;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override List<OutputOrderingSetting> GetChoices(FilterConfiguration configuration)
    {
        return new List<OutputOrderingSetting>()
        {
            OutputOrderingSetting.AsAdded,
            OutputOrderingSetting.ByName,
            OutputOrderingSetting.ByClass,
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, OutputOrderingSetting choice)
    {
        switch (choice)
        {
            case(OutputOrderingSetting.AsAdded):
                return "As Added";
            case(OutputOrderingSetting.ByName):
                return "By Name";
            case(OutputOrderingSetting.ByClass):
                return "By Class";
        }

        return choice.ToString();
    }

    public CraftOutputOrderingFilter(ILogger<CraftOutputOrderingFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}