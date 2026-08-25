using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class HighlightingLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("highlighting", "Highlighting",
            Paragraph(
                "Highlighting tints inventory slots and bag tabs when a list matches the items inside them. " +
                "These are the defaults, individual lists can override most of them."),
            Section("Which lists highlight",
                Paragraph(
                    "Highlighting is driven by a list. These pick which list applies where, and are " +
                    "upstream of everything else on this page."),
                Setting<BackgroundFilterSetting>("In the game's own inventory windows"),
                Setting<WindowFilterSetting>("In Allagan Tools' windows"),
                Setting<SaveBackgroundFilterSetting>("Remember the choice between sessions")),
            Section("When to highlight",
                Setting<HighlightWhenSetting>(),
                Setting<InvertHighlightingSetting>(),
                Setting<InvertTabHighlightingSetting>()),
            Section("Colours",
                Setting<HighlightColourSetting>("Matched items"),
                Setting<TabHighlightColourSetting>("Bag tabs containing matches")),
            Section("Destinations",
                Paragraph(
                    "Sort lists know where items are meant to end up. The destination bag can be highlighted " +
                    "alongside the source so you can see both ends of a move at once."),
                Setting<HighlightDestinationSetting>(),
                EnabledBy<HighlightDestinationSetting>(
                    Setting<HighlightDestinationEmptySetting>(),
                    Setting<InvertDestinationHighlightingSetting>(),
                    Setting<HighlightDestinationColourSetting>("Destination colour"))),
            Section("Retainer list",
                Paragraph(
                    "The summoning bell list can be annotated when a retainer holds items one of your lists " +
                    "cares about, so you know which to open without checking each one."),
                Setting<ColourRetainerListSetting>("Colour retainer names"),
                EnabledBy<ColourRetainerListSetting>(
                    Setting<RetainerListColourSetting>("Name colour")),
                Setting<ShowItemNumberRetainerListSetting>("Show item counts")),
            Section("Shop highlighting",
                Paragraph(
                    "While a vendor window is open, shop items matched by a list can be highlighted, and the " +
                    "vendors that sell them can be marked out in the world."),
                Setting<ShopHighlightingDisableItemsSetting>("Dim items that don't match"),
                Setting<ShopHighlightingNpcSetting>("Highlight vendor NPCs in the world"),
                EnabledBy<ShopHighlightingNpcSetting>(
                    Setting<ShopHighlightingNpcColorSetting>("Highlight colour"),
                    Setting<ShopHighlightingNpcNameplateIconSetting>("Show an icon on their nameplate")))
        );
    }
}