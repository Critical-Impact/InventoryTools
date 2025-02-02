using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ToolTipLocationLimitSetting : IntegerSetting
{
    public override int DefaultValue { get; set; } = 10;
    public override int CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipLocationLimit;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
    {
        configuration.TooltipLocationLimit = newValue;
    }

    public override string Key { get; set; } = "TooltipLocationLimit";
    public override string Name { get; set; } = "Add Item Locations (Max Results)";
    public override string HelpText { get; set; } = "The maximum amount of locations to list on the tooltip. This requires 'Display Amount Owned?' to be enabled.";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AddItemLocations;
    public override string Version => "1.7.0.0";

    public ToolTipLocationLimitSetting(ILogger<ToolTipLocationLimitSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}