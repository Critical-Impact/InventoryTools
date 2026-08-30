using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class ContextMenuLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("context-menu", "Context Menu",
            Paragraph("Entries Allagan Tools adds to the game's own right-click menus. Each one costs a line in menus you already use, so they are individually switchable."),
            Section("Lists",
                Setting<ContextMenuAddToCraftListSetting>("Add to craft list"),
                Setting<ContextMenuAddToActiveCraftListSetting>("Add to the active craft list"),
                Setting<ContextMenuAddToCuratedListSetting>("Add to curated list"),
                Setting<ContextMenuAddToFavouritesSetting>("Add or remove from favourites")),
            Section("More information",
                Paragraph("Opens the Allagan Tools information window for whatever was right-clicked."),
                Setting<ContextMenuMoreInformationSetting>("Items"),
                Setting<ContextMenuMoreInformationNpcsSetting>("NPCs"),
                Setting<ContextMenuMoreInformationMonstersSetting>("Monsters")),
            Section("Open in game",
                Paragraph("Shortcuts to open the various logs within the game."),
                Setting<ContextMenuOpenCraftingLogSetting>("Crafting log"),
                Setting<ContextMenuOpenGatheringLogSetting>("Gathering log"),
                Setting<ContextMenuOpenFishingLogSetting>("Fishing log")),
            Section("Search",
                Paragraph("The search locations also apply to searches started from Allagan Tools' own menus, not just this context menu entry."),
                Paragraph("This search is more expansive than the game's default search functionality as it searches across every character the plugin knows about."),
                Setting<ContextMenuItemSearchSetting>("Search for this item"),
                Paragraph("If you want to limit the locations it searches, configure the search scope below."),
                Setting<ContextMenuItemSearchScopeSetting>("Search these locations")),
            Section("Other",
                Setting<ContextMenuCopyNameSetting>("Copy item name"))
        );
    }
}
