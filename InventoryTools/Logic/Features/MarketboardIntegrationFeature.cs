using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class MarketboardIntegrationFeature : Feature
{
    public MarketboardIntegrationFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/marketboard", "Market Board",
            Paragraph("Prices come from Universalis, not from the game. The plugin downloads the prices and keeps a local copy. When this is on, you can filter and sort your lists by price."),
            Setting<AutomaticallyDownloadPricesSetting>("Download prices automatically"),
            Setting<MarketRefreshTimeHoursSetting>("Keep prices for this many hours"),
            Setting<MarketBoardSaleCountLimitSetting>("Days of sale history to include")
        );
    }
}
