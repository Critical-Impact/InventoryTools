using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class AutoSaveSetting : BooleanSetting
    {
        private readonly PluginLogic _pluginLogic;

        public AutoSaveSetting(ILogger<AutoSaveSetting> logger, ImGuiService imGuiService, PluginLogic pluginLogic) : base(logger, imGuiService)
        {
            _pluginLogic = pluginLogic;
        }
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AutoSave;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AutoSave = newValue;
            _pluginLogic.ClearAutoSave();
        }

        public override string Key { get; set; } = "AutoSave";
        public override string Name { get; set; } = "Auto save inventories/configuration?";

        public override string WizardName { get; } = "Auto save inventories?";

        public override string HelpText { get; set; } =
            "Should the inventories/configuration be automatically saved on a defined interval? While the plugin does save when the game is closed and when configurations are altered, it is not saved in cases of crashing so this attempts to alleviate this.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AutoSave;
        public override string Version => "1.6.2.5";
    }
}