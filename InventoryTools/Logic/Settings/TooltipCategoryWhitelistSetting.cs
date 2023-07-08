using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using Dalamud.Utility;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings;

public class TooltipCategoryWhitelistSetting : MultipleChoiceSetting<uint>
{
    public override List<uint> DefaultValue { get; set; } = new();
    public override List<uint> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipWhitelistCategories;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<uint> newValue)
    {
        configuration.TooltipWhitelistCategories = newValue;
    }

    public override string Key { get; set; } = "TooltipWhitelistCategories";
    public override string Name { get; set; } = "Tooltip Category Whitelist";
    public override string HelpText { get; set; } = "When adding information to tooltips, should we limit the items affected to these categories? If 'Tooltip Category Blacklist` is checked, this functionality will be reversed.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override Dictionary<uint, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        return Service.ExcelCache.GetItemUICategorySheet()
            .ToDictionary(c => c.RowId, c => c.Name.ToDalamudString().ToString());
    }

    public override bool HideAlreadyPicked { get; set; } = true;
}