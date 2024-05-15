using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class SwitchFiltersAutomaticallySetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.SwitchFiltersAutomatically;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.SwitchFiltersAutomatically = newValue;
        }

        public override string Key { get; set; } = "SwitchFiltersAutomatically";
        public override string Name { get; set; } = "Switch lists automatically?";

        public override string HelpText { get; set; } =
            "When you view a different list, should highlighting automatically switch to the list you are viewing? Highlighting will only change to the new list if highlighting is already active.";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Lists;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public SwitchFiltersAutomaticallySetting(ILogger<SwitchFiltersAutomaticallySetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}