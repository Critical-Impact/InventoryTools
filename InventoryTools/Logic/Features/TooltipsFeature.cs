using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class TooltipsFeature : Feature
{
    public TooltipsFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/tooltips", "Tooltips",
            Paragraph(
                "Allagan Tools can add extra lines to the game's item tooltips. Each option below adds one line. Select the lines that you want."),
            Setting<TooltipDisplayAmountOwnedSetting>("Where you own the item"),
            Setting<TooltipDisplayRetrieveAmountSetting>("How many the active craft list still needs"),
            Setting<TooltipMinimumMarketPriceSetting>("The market price"),
            Setting<TooltipDisplayUnlockSetting>("Whether you have learned the item"),
            Setting<TooltipSourceInformationEnabledSetting>("Where the item comes from"),
            Setting<TooltipUseInformationEnabledSetting>("What the item is used for"),
            Setting<TooltipDisplayIngredientPatchSetting>("Which patch an ingredient is from"),
            Setting<TooltipDisplayCofferLootSetting>("What a coffer can contain"),
            Setting<TooltipDisplayGlamourReadySetSetting>("Whether the item completes an outfit"),
            Paragraph("Each line has more options in the settings window, under Tooltips. The options include colours, the locations to search, and the display mode.")
        );
    }
}
