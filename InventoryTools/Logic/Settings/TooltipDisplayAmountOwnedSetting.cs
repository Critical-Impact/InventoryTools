using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class TooltipDisplayAmountOwnedSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipDisplayAmountOwned;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.TooltipDisplayAmountOwned = newValue;
        }

        public override string Key { get; set; } = "TooltipDisplayOwned";
        public override string Name { get; set; } = "Display Amount Owned?";

        public override string HelpText { get; set; } =
            "When hovering an item, should the tooltip contain information about where the items are located.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Tooltips;
    }
}