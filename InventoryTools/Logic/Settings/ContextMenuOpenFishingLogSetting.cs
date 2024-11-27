using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ContextMenuOpenFishingLogSetting : BooleanSetting
{
    public ContextMenuOpenFishingLogSetting(ILogger<ContextMenuOpenFishingLogSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.OpenFishingLogContextMenu;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.OpenFishingLogContextMenu = newValue;
    }

    public override string Key { get; set; } = "OpenFishingLogContextMenu";
    public override string Name { get; set; } = "Context Menu - Open Fishing Log";

    public override string WizardName { get; } = "Open Fishing Log";

    public override string HelpText { get; set; } =
        "Add a context menu item to open the fishing log for any item that can be fished?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version { get; } = "1.11.0.2";
}