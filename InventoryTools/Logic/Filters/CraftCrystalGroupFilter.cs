using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

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

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        return null;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.SetCrystalGroupSetting(EmptyValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CrystalGroupSetting newValue)
    {
        configuration.CraftList.SetCrystalGroupSetting(newValue);
    }

    public override string Key { get; set; } = "CraftCrystalGroupFilter";
    public override string Name { get; set; } = "Group Crystals By";

    public override string HelpText { get; set; } =
        "Should the crystals be grouped together or show up in the Gather/Buy list?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override CrystalGroupSetting EmptyValue { get; set; } = CrystalGroupSetting.Separate;
    public override List<CrystalGroupSetting> GetChoices(FilterConfiguration configuration)
    {
        return new List<CrystalGroupSetting>()
        {
            CrystalGroupSetting.Separate,
            CrystalGroupSetting.Together
        };
    }

    public override string GetFormattedChoice(CrystalGroupSetting choice)
    {
        return choice.ToString();
    }
}