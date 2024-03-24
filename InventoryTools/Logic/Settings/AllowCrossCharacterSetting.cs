using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class AllowCrossCharacterSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.DisplayCrossCharacter;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.DisplayCrossCharacter = newValue;
        }

        public override string Key { get; set; } = "DisplayCrossCharacter";
        public override string Name { get; set; } = "Allow Cross-Character Inventories?";
        public override string WizardName { get; } = "Cross-Character Inventories?";

        public override string HelpText { get; set; } =
            "Should characters not currently logged in and their associated retainers be available to view/search and filter against?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.6.2.5";

        public AllowCrossCharacterSetting(ILogger<AllowCrossCharacterSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}