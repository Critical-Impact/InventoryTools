using CriticalCommonLib.MarketBoard;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class MarketRefreshTimeHoursSetting : IntegerSetting
    {
        private readonly MarketCacheConfiguration _marketCacheConfiguration;

        public MarketRefreshTimeHoursSetting(ILogger<MarketRefreshTimeHoursSetting> logger, ImGuiService imGuiService, MarketCacheConfiguration marketCacheConfiguration) : base(logger, imGuiService)
        {
            _marketCacheConfiguration = marketCacheConfiguration;
        }

        public override int DefaultValue { get; set; } = 24;
        public override int CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.MarketRefreshTimeHours;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
        {
            configuration.MarketRefreshTimeHours = newValue;
            _marketCacheConfiguration.CacheMaxAgeHours = newValue;
        }

        public override string Key { get; set; } = "MarketRefreshTime";
        public override string Name { get; set; } = "Keep market prices for X hours";

        public override string WizardName { get; } = "Persist for X hours";
        public override string HelpText { get; set; } = "How long should we store the market prices for before refreshing from universalis?";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.MarketBoard;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Market;
        public override string Version => "1.7.0.0";
    }
}