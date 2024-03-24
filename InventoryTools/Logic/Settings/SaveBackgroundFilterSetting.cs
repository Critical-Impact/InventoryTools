using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class SaveBackgroundFilterSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.SaveBackgroundFilter;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.SaveBackgroundFilter = newValue;
        }

        public override string Key { get; set; } = "SaveBackgroundFilter";
        public override string Name { get; set; } = "Save Background Filter?";

        public override string HelpText { get; set; } =
            "Should the active background filter be saved when exiting the game or disabling/re-enabling the plugin?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;

        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.FilterSettings;
        public override string Version => "1.6.2.5";

        public SaveBackgroundFilterSetting(ILogger<SaveBackgroundFilterSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}