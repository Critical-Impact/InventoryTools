using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class InvertTabHighlightingSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.InvertTabHighlighting;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.InvertTabHighlighting = newValue;
        }

        public override string Key { get; set; } = "InvertTabHighlighting";
        public override string Name { get; set; } = "Invert Tab Highlighting?";

        public override string HelpText { get; set; } =
            "Should all the tabs not matching a filter be highlighted instead? This can be overridden in the filter configuration.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Highlighting;

    }
}