using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

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
        public override string Name { get; set; } = "Show 'All Lists' Tab?";

        public override string HelpText { get; set; } =
            "Should the main window show the tab called 'All Lists' containing all available lists in one tab?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Windows;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.6.2.5";

        public ShowFiltersTabSetting(ILogger<ShowFiltersTabSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}