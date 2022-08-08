using System;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class MarketBoardSaleCountLimitSetting : IntegerSetting
    {
        public override int DefaultValue { get; set; } = 7;
        public override int CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.MarketSaleHistoryLimit;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
        {
            newValue = Math.Min(30, newValue);
            newValue = Math.Max(1, newValue);
            configuration.MarketSaleHistoryLimit = newValue;
        }

        public override string Key { get; set; } = "MBSaleCountLimit";
        public override string Name { get; set; } = "Marketboard Sale History Days";

        public override string HelpText { get; set; } =
            "When calculating the total number of sales for an item, this is how many days back should be examined for sales data to calculate that number. If you change this, the existing data will not be wiped, you will need to either manually request a refresh of MB prices OR wait for the marketboard refresh to happen automatically.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.MarketBoard;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Market;
    }
}