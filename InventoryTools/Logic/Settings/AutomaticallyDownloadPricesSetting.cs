using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class AutomaticallyDownloadPricesSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AutomaticallyDownloadMarketPrices;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AutomaticallyDownloadMarketPrices = newValue;
        }

        public override string Key { get; set; } = "AutomaticallyDownloadPrices";
        public override string Name { get; set; } = "Automatically download prices?";

        public override string WizardName { get; } = "Download Pricing Data";
        public override string HelpText { get; set; } = "Should price data be automatically downloaded when it's viewed in a list?";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.MarketBoard;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Market;
        public override string Version => "1.7.0.0";

        public AutomaticallyDownloadPricesSetting(ILogger<AutomaticallyDownloadPricesSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}