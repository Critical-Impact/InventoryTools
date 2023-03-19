using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;

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
    public override string HelpText { get; set; } = "Set the layout of the craft window";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.WindowLayout;

    public override Dictionary<WindowLayout, string> Choices { get; } = new Dictionary<WindowLayout, string>()
    {
        { WindowLayout.Sidebar, "Sidebar" },
        { WindowLayout.Tabs , "Tabs" }
    };
}