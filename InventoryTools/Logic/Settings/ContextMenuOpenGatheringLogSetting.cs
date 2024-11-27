using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ContextMenuOpenGatheringLogSetting : BooleanSetting
{
    public ContextMenuOpenGatheringLogSetting(ILogger<ContextMenuOpenGatheringLogSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.OpenGatheringLogContextMenu;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.OpenGatheringLogContextMenu = newValue;
    }

    public override string Key { get; set; } = "OpenGatheringLogContextMenu";
    public override string Name { get; set; } = "Context Menu - Open Gathering Log";

    public override string WizardName { get; } = "Open Gathering Log";

    public override string HelpText { get; set; } =
        "Add a context menu item to open the gathering log for any item that can be gathered?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version { get; } = "1.11.0.2";
}