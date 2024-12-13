using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftEverythingElseGroupFilter : ChoiceFilter<EverythingElseGroupSetting>
{
    public override EverythingElseGroupSetting CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.EverythingElseGroupSetting;
    }

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.SetEverythingElseGroupSetting(DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, EverythingElseGroupSetting newValue)
    {
        configuration.CraftList.SetEverythingElseGroupSetting(newValue);
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftEverythingElseGroupFilter";
    public override string Name { get; set; } = "Group Everything Else By";

    public override string HelpText { get; set; } =
        "How should everything else not in it's own group be grouped?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override EverythingElseGroupSetting DefaultValue { get; set; } = EverythingElseGroupSetting.ByClosestZone;
    public override List<EverythingElseGroupSetting> GetChoices(FilterConfiguration configuration)
    {
        return new List<EverythingElseGroupSetting>()
        {
            EverythingElseGroupSetting.Together,
            EverythingElseGroupSetting.ByClosestZone
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration,
        EverythingElseGroupSetting choice)
    {
        switch (choice)
        {
            case EverythingElseGroupSetting.Together:
                return "Together";
            case EverythingElseGroupSetting.ByClosestZone:
                return "Zone";
        }
        return choice.ToString();
    }

    public CraftEverythingElseGroupFilter(ILogger<CraftEverythingElseGroupFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}