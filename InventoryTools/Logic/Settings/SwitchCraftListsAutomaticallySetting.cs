using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class SwitchCraftListsAutomaticallySetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.SwitchCraftListsAutomatically;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.SwitchCraftListsAutomatically = newValue;
        }

        public override string Key { get; set; } = "SwitchCraftListsAutomatically";
        public override string Name { get; set; } = "Switch craft lists automatically?";

        public override string HelpText { get; set; } =
            "Should the active craft list automatically change when moving between each craft list? The active craft list will only change if there is an active craft list already selected.";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Lists;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.6.2.5";

        public SwitchCraftListsAutomaticallySetting(ILogger<SwitchCraftListsAutomaticallySetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}