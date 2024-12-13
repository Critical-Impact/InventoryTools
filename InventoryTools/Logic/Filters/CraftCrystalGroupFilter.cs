using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftCrystalGroupFilter : ChoiceFilter<CrystalGroupSetting>
{
    public override CrystalGroupSetting CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CrystalGroupSetting;
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
        configuration.CraftList.SetCrystalGroupSetting(DefaultValue);
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CrystalGroupSetting newValue)
    {
        configuration.CraftList.SetCrystalGroupSetting(newValue);
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftCrystalGroupFilter";
    public override string Name { get; set; } = "Group Crystals By";

    public override string HelpText { get; set; } =
        "Should the crystals be grouped together or show up in the Gather/Buy list?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override CrystalGroupSetting DefaultValue { get; set; } = CrystalGroupSetting.Separate;
    public override List<CrystalGroupSetting> GetChoices(FilterConfiguration configuration)
    {
        return new List<CrystalGroupSetting>()
        {
            CrystalGroupSetting.Separate,
            CrystalGroupSetting.Together
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CrystalGroupSetting choice)
    {
        return choice.ToString();
    }

    public CraftCrystalGroupFilter(ILogger<CraftCrystalGroupFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}