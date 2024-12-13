using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftCurrencyGroupFilter : ChoiceFilter<CurrencyGroupSetting>
{
    public override CurrencyGroupSetting CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CurrencyGroupSetting;
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
        configuration.CraftList.SetCurrencyGroupSetting(DefaultValue);
        configuration.NotifyConfigurationChange();
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CurrencyGroupSetting newValue)
    {
        configuration.CraftList.SetCurrencyGroupSetting(newValue);
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftCurrencyGroupFilter";
    public override string Name { get; set; } = "Group Currency By";

    public override string HelpText { get; set; } =
        "Should the currency be grouped together or show up in the Gather/Buy list?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override CurrencyGroupSetting DefaultValue { get; set; } = CurrencyGroupSetting.Separate;
    public override List<CurrencyGroupSetting> GetChoices(FilterConfiguration configuration)
    {
        return new List<CurrencyGroupSetting>()
        {
            CurrencyGroupSetting.Separate,
            CurrencyGroupSetting.Together
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CurrencyGroupSetting choice)
    {
        return choice.ToString();
    }

    public CraftCurrencyGroupFilter(ILogger<CraftCurrencyGroupFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}