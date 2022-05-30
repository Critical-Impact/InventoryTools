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
        public override string Name { get; set; } = "Display Market Minimum Price?";

        public override string HelpText { get; set; } =
            "When hovering an item, should the tooltip contain the average market price for NQ/HQ.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Tooltips;
    }
}