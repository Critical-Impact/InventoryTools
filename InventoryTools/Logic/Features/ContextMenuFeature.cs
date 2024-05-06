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
        },
        settings)
    {
    }

    public override string Name { get; } = "Context Menus";

    public override string Description { get; } =
        "Adds new items to the right click/context menu for items in the game. 'More Information' will open a window that provides you with how to acquire the item, how to use it and a myriad of other information. 'Add to Craft List' opens a submenu with all your craft lists, allowing you to quickly add items to your craft lists.";
}