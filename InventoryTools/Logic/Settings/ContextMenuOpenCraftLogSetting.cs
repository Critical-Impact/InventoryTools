using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ContextMenuOpenCraftingLogSetting : BooleanSetting
{
    public ContextMenuOpenCraftingLogSetting(ILogger<ContextMenuOpenCraftingLogSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.OpenCraftingLogContextMenu;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.OpenCraftingLogContextMenu = newValue;
    }

    public override string Key { get; set; } = "OpenCraftingLogContextMenu";
    public override string Name { get; set; } = "Context Menu - Open Crafting Log";

    public override string WizardName { get; } = "Open Crafting Log";

    public override string HelpText { get; set; } =
        "Add a context menu item to open the crafting log for any item that can be crafted?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    public override string Version { get; } = "1.11.0.2";
}