using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class ContextMenuFeature : Feature
{
    public ContextMenuFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(MoreInfoContextMenuSetting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Context Menus";

    public override string Description { get; } =
        "Adds an new menu item to the menus that show up when you right click on an item. At present 'More Information' is the only option. Hitting 'More Information' will open a window that provides you with how to acquire the item, how to use it and a myriad of other information.";
}