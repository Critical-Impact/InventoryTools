using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class HistoryEnabledSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.HistoryEnabled;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.HistoryEnabled = newValue;
    }

    public override string Key { get; set; } = "HistoryEnabled";
    public override string Name { get; set; } = "Enable History Tracking?";
    public override string WizardName { get; } = "Track Item History?";

    public override string HelpText { get; set; } =
        "Should Allagan Tools attempt to track the movement, addition and removal of items in your inventories?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.History;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version => "1.7.0.0";

    public HistoryEnabledSetting(ILogger<HistoryEnabledSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}