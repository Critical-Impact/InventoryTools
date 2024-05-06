using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class MarketboardIntegrationFeature : Feature
{
    public MarketboardIntegrationFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(AutomaticallyDownloadPricesSetting),
            typeof(MarketRefreshTimeHoursSetting),
            typeof(MarketBoardSaleCountLimitSetting),
        },
        settings)
    {
    }
    public override string Name { get; } = "Marketboard";
    public override string Description { get; } =
        "Configure the marketboard integration. This downloads data from Universalis on a set timer, allowing you to filter against the minimum and average prices of items across multiple servers.";
}