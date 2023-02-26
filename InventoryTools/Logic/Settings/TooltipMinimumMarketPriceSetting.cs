using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class TooltipMinimumMarketPriceSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipDisplayMarketLowestPrice;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.TooltipDisplayMarketLowestPrice = newValue;
        }

        public override string Key { get; set; } = "TooltipDisplayMBMinimum";
        public override string Name { get; set; } = "Add Market Minimum NQ/HQ Price?";

        public override string HelpText { get; set; } =
            "When hovering an item, should the tooltip contain the minimum market price for both NQ and HQ. Please make sure 'Automatically download prices' is enabled.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Subsetting;
    }
}