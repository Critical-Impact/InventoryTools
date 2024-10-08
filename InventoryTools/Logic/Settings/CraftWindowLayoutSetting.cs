using System.Collections.Generic;
using System.Numerics;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class CraftWindowLayoutSetting : ChoiceSetting<WindowLayout>
{
    public override WindowLayout DefaultValue { get; set; } = WindowLayout.Tabs;
    public override WindowLayout CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.CraftWindowLayout;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, WindowLayout newValue)
    {
        configuration.CraftWindowLayout = newValue;
    }

    public override string Key { get; set; } = "CraftWindowLayout";
    public override string Name { get; set; } = "Craft Window Layout";

    public override string WizardName { get; } = "Craft Window";
    public override string HelpText { get; set; } = "Set the layout of the craft window";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Windows;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.WindowLayout;

    public override string? Image { get; } = "craft_display";

    public override Vector2? ImageSize { get; } = new Vector2(878, 393);

    public override Dictionary<WindowLayout, string> Choices { get; } = new Dictionary<WindowLayout, string>()
    {
        { WindowLayout.Sidebar, "Sidebar" },
        { WindowLayout.Tabs , "Tabs" },
        { WindowLayout.Single , "Single" }
    };
    public override string Version => "1.7.0.0";

    public CraftWindowLayoutSetting(ILogger<CraftWindowLayoutSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}