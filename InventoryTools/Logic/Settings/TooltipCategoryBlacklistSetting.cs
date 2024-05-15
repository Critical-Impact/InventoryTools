using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipCategoryBlacklistSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; }
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipWhitelistBlacklist;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipWhitelistBlacklist = newValue;
    }

    public override string Key { get; set; } = "TooltipCategoryBlacklist";
    public override string Name { get; set; } = "Tooltip Category Blacklist";
    public override string HelpText { get; set; } = "Makes the Tooltip Category Whitelist into a Blacklist if checked.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version => "1.7.0.0";

    public TooltipCategoryBlacklistSetting(ILogger<TooltipCategoryBlacklistSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}