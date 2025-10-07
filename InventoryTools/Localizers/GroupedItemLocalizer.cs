using System;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using InventoryTools.Logic;

namespace InventoryTools.Localizers;

public class GroupedItemLocalizer
{
    public GroupedItemLocalizer()
    {
    }

    public string FormattedName(GroupedItemGroup groupedItemGroup)
    {
        return groupedItemGroup switch
        {
            GroupedItemGroup.None => "None",
            GroupedItemGroup.World => "World",
            GroupedItemGroup.IsHq => "Is HQ",
            GroupedItemGroup.Owner => "Owner",
            _ => groupedItemGroup.ToString(),
        };
    }
}