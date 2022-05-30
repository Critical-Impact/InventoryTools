using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class ShowFiltersTabSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ShowFilterTab;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.ShowFilterTab = newValue;
        }

        public override string Key { get; set; } = "ShowFiltersTab";
        public override string Name { get; set; } = "Show Filter Tab?";

        public override string HelpText { get; set; } =
            "Should the main window show the tab called 'Filters' that lists all the available filters in one screen?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.FilterSettings;

    }
}