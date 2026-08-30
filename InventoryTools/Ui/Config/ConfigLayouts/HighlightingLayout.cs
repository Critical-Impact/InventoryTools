using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class HighlightingLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("highlighting", "Highlighting",
            Paragraph("Highlighting tints inventory slots and bag tabs when a list matches the items inside them. These are the defaults, individual lists can override most of them."),
            Section("Active lists",
                Paragraph("Highlighting is generally controlled by hitting the 'Highlight' button within the various windows and via slash commands, you can toggle highlight on/off here as well."),
                Setting<WindowFilterSetting>(),
                Setting<BackgroundFilterSetting>(),
                Setting<SaveBackgroundFilterSetting>("Remember the choice between sessions")
            ),
            Section("When to highlight",
                Setting<HighlightWhenSetting>(),
                Setting<InvertHighlightingSetting>(),
                Setting<InvertTabHighlightingSetting>()),
            Section("Colours",
                Setting<HighlightColourSetting>("Matched item colour"),
                Setting<TabHighlightColourSetting>("Matching tab colour")),
            Section("Destinations",
                Paragraph("Sort lists know where items are meant to end up. The destination bag can be highlighted alongside the source so you can see both ends of a move at once."),
                Setting<HighlightDestinationSetting>(),
                EnabledBy<HighlightDestinationSetting>(
                    Setting<HighlightDestinationEmptySetting>(),
                    Setting<InvertDestinationHighlightingSetting>(),
                    Setting<HighlightDestinationColourSetting>("Destination colour"))),
            Section("Retainer list",
                Paragraph("The summoning bell list can be annotated when a retainer holds items one of your lists cares about, so you know which to open without checking each one."),
                Setting<ColourRetainerListSetting>("Colour retainer names"),
                EnabledBy<ColourRetainerListSetting>(
                    Setting<RetainerListColourSetting>("Name colour")),
                Setting<ShowItemNumberRetainerListSetting>("Show item counts")),
            Section("Shop highlighting",
                Paragraph("While a vendor window is open, shop items matched by a list can be highlighted, and the vendors that sell them can be marked out in the world."),
                Setting<ShopHighlightingDisableItemsSetting>("Dim items that don't match"),
                Setting<ShopHighlightingNpcSetting>("Highlight vendor NPCs in the world"),
                EnabledBy<ShopHighlightingNpcSetting>(
                    Setting<ShopHighlightingNpcColorSetting>("Highlight colour"),
                    Setting<ShopHighlightingNpcNameplateIconSetting>("Show an icon on their nameplate")))
        );
    }
}