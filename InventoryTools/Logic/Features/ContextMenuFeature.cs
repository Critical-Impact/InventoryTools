using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class ContextMenuFeature : Feature
{
    public ContextMenuFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(ContextMenuMoreInformationSetting),
            typeof(ContextMenuAddToCraftListSetting),
            typeof(ContextMenuAddToActiveCraftListSetting),
            typeof(ContextMenuAddToCuratedListSetting),
            typeof(ContextMenuOpenCraftingLogSetting),
            typeof(ContextMenuOpenGatheringLogSetting),
            typeof(ContextMenuOpenFishingLogSetting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Context Menus";

    public override string Description { get; } =
        "Adds new items to the right click/context menu for items in the game. ";
}