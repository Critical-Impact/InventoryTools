using InventoryTools.Logic.Settings.Abstract;

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

        public override string HelpText { get; set; } =
            "This is an experimental feature, should characters not currently logged in and their associated retainers be shown in filter configurations?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Experimental;
    }
}