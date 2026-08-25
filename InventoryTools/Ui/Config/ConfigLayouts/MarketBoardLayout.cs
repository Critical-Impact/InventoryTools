using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class MarketBoardLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("marketboard", "Market Board",
            Paragraph("Market prices come from Universalis rather than the game, so they are fetched and cached rather than read live."),
            Section("Worlds to price",
                Paragraph("These combine into a single list, the extra worlds below are added to your home and current world rather than replacing them."),
                Setting<MarketBoardUseHomeWorldSetting>("Home world"),
                Setting<MarketBoardUseActiveWorldSetting>("Current world"),
                Setting<MarketBoardExtraWorldsSetting>("Additional worlds")),
            Section("Downloading",
                Setting<AutomaticallyDownloadPricesSetting>("Download prices automatically"),
                Setting<MarketRefreshTimeHoursSetting>("Keep prices for (hours)"),
                Setting<MarketBoardSaleCountLimitSetting>("Sale history window (days)"))
        );
    }
}
