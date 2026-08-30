using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class ContextMenuFeature : Feature
{
    public ContextMenuFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/context-menu", "Context Menus",
            Paragraph("The plugin can add these entries to item's right-click menus."),
            Section("More information",
                Setting<ContextMenuMoreInformationSetting>("Items"),
                Setting<ContextMenuMoreInformationNpcsSetting>("NPCs"),
                Setting<ContextMenuMoreInformationMonstersSetting>("Monsters")),
            Section("Open a game log",
                Setting<ContextMenuOpenCraftingLogSetting>("Crafting log"),
                Setting<ContextMenuOpenGatheringLogSetting>("Gathering log"),
                Setting<ContextMenuOpenFishingLogSetting>("Fishing log")),
            Section("Lists",
                Setting<ContextMenuAddToCraftListSetting>("Add to a craft list"),
                Setting<ContextMenuAddToActiveCraftListSetting>("Add to the active craft list"),
                Setting<ContextMenuAddToCuratedListSetting>("Add to a curated list"),
                Setting<ContextMenuAddToFavouritesSetting>("Add to or remove from favourites")),
            Section("Other",
                Setting<ContextMenuCopyNameSetting>("Copy the item name"))
        );
    }
}
