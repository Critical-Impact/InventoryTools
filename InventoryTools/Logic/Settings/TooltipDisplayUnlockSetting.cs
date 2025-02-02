using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayUnlockSetting : BooleanSetting
{
    public TooltipDisplayUnlockSetting(ILogger<TooltipDisplayUnlockSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipDisplayUnlock;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipDisplayUnlock = newValue;
    }

    public override string Key { get; set; } = "TooltipDisplayUnlockSetting";
    public override string Name { get; set; } = "Add Item Unlock Status";
    public override string HelpText { get; set; } = "If an item can be unlocked/acquired, shows if your characters have unlocked/acquired said item. Can be configured to show specific characters inside the configuration window.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.ItemUnlockStatus;
    public override string Version { get; } = "1.11.0.4";
}