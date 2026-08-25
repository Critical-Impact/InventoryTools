using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class TooltipsLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return PageGroup("tooltips", "Tooltips",
            Page("tooltips/general", "General",
                Paragraph("Allagan Tools can add extra lines to the game's item tooltips. Every feature in this section is inactive unless tooltip tweaks are enabled."),
                Setting<ShowTooltipsSetting>("Enable tooltip tweaks"),
                EnabledBy<ShowTooltipsSetting>(
                    Setting<ImGuiTooltipModeSetting>("Show in Allagan Tools' own windows"),
                    Setting<TooltipDisplayHeaderSetting>("Label added lines with the plugin name"),
                    Setting<TooltipCategoryWhitelistSetting>("Limit to these item categories"),
                    Setting<TooltipCategoryBlacklistSetting>("Treat the list above as exclusions"))),
            Page("tooltips/appearance", "Appearance",
                Paragraph("Defaults for the lines Allagan Tools adds. Each feature can override the colour."),
                Setting<TooltipColorSetting>("Default text colour"),
                Setting<TooltipHeaderLinesSetting>("Blank lines above"),
                Setting<TooltipFooterLinesSetting>("Blank lines below")),
            Page("tooltips/locations", "Item locations",
                Paragraph("Where you already own this item, and how many."),
                Setting<TooltipDisplayAmountOwnedSetting>("Show where I own this"),
                EnabledBy<TooltipDisplayAmountOwnedSetting>(
                    Setting<TooltipLocationScopeLimitSetting>("Search these locations"),
                    Setting<TooltipLocationDisplayModeSetting>("Display mode"),
                    Setting<TooltipAmountOwnedSortSetting>("Order"),
                    Setting<ToolTipLocationLimitSetting>("Maximum results"),
                    Setting<TooltipCurrentCharacterSetting>("Current character only"),
                    Setting<TooltipAddCharacterNameSetting>("Affix the character name"),
                    Setting<TooltipAmountOwnedColorSetting>("Text colour"))),
            Page("tooltips/retrieve", "Amount to retrieve",
                Paragraph("How many of this item your active list still wants."),
                Setting<TooltipDisplayRetrieveAmountSetting>("Show amount to retrieve"),
                EnabledBy<TooltipDisplayRetrieveAmountSetting>(
                    Setting<TooltipAmountToRetrieveColorSetting>("Text colour"))),
            Page("tooltips/market", "Market prices",
                Paragraph("Universalis pricing for the item. Either line can be shown on its own."),
                Setting<TooltipAverageMarketPriceSetting>("Show average NQ/HQ price"),
                Setting<TooltipMinimumMarketPriceSetting>("Show minimum NQ/HQ price"),
                Setting<TooltipMarketPricingColorSetting>("Text colour")),
            Page("tooltips/unlock", "Item unlock status",
                Paragraph("Whether the item has been learned, and by whom."),
                Setting<TooltipDisplayUnlockSetting>("Show unlock status"),
                EnabledBy<TooltipDisplayUnlockSetting>(
                    Setting<TooltipDisplayUnlockCharacterSetting>("Characters to check"),
                    Setting<TooltipDisplayUnlockDisplayModeSetting>("Display mode"),
                    Setting<TooltipDisplayUnlockHideUnlockedSetting>("Hide characters who have it"),
                    Setting<TooltipItemUnlockStatusColorSetting>("Text colour"))),
            Page("tooltips/coffer", "Coffer loot",
                Paragraph("For coffers and containers, what they can contain."),
                Setting<TooltipDisplayCofferLootSetting>("Show coffer contents"),
                EnabledBy<TooltipDisplayCofferLootSetting>(
                    Setting<TooltipCofferLootScopeSetting>("Search these locations"),
                    Setting<TooltipCofferLootColorSetting>("Text colour"))),
            Page("tooltips/glamour", "Outfit glamour",
                Paragraph("Whether this item completes an outfit you are collecting."),
                Setting<TooltipDisplayGlamourReadySetSetting>("Show outfit glamour info"),
                EnabledBy<TooltipDisplayGlamourReadySetSetting>(
                    Setting<TooltipGlamourReadySetScopeSetting>("Search these locations"),
                    Setting<TooltipGlamourReadySetDisplayModeSetting>("Display mode"),
                    Setting<TooltipGlamourReadySetColorSetting>("Text colour"),
                    Setting<TooltipGlamourReadySetAcquiredColorSetting>("Acquired item colour"),
                    Setting<TooltipGlamourReadySetNotAcquiredColorSetting>("Not acquired item colour"))),
            Page("tooltips/patch", "Ingredient patch",
                Paragraph("Which patch a crafting ingredient was introduced in."),
                Setting<TooltipDisplayIngredientPatchSetting>("Show ingredient patch"),
                EnabledBy<TooltipDisplayIngredientPatchSetting>(
                    Setting<TooltipIngredientPatchTooltipColorSetting>("Text colour"))),
            Page("tooltips/sources", "Source information",
                Paragraph("How the item can be acquired. Pick which sources are worth a tooltip line."),
                Setting<TooltipSourceInformationEnabledSetting>("Show source information"),
                EnabledBy<TooltipSourceInformationEnabledSetting>(
                    Setting<TooltipSourceInformationModifierSetting>("Hold this key to show"),
                    Setting<TooltipSourceInformationColorSetting>("Text colour"),
                    Scrollable("sourceInformation", 260,
                        Setting<TooltipSourceInformationSetting>()))),
            Page("tooltips/uses", "Use information",
                Paragraph("What the item can be turned into or spent on."),
                Setting<TooltipUseInformationEnabledSetting>("Show use information"),
                EnabledBy<TooltipUseInformationEnabledSetting>(
                    Setting<TooltipUseInformationModifierSetting>("Hold this key to show"),
                    Setting<TooltipUseInformationColorSetting>("Text colour"),
                    Scrollable("useInformation", 260,
                        Setting<TooltipUseInformationSetting>())))
        );
    }
}