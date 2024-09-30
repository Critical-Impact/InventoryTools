using System;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Extensions;

public static class ItemFlagsExtensions
{
    public static string FormattedName(this InventoryItem.ItemFlags flags)
    {
        return flags switch
        {
            InventoryItem.ItemFlags.None => "None",
            InventoryItem.ItemFlags.HighQuality => "High Quality",
            InventoryItem.ItemFlags.CompanyCrestApplied => "Company Crest Applied",
            InventoryItem.ItemFlags.Relic => "Relic",
            InventoryItem.ItemFlags.Collectable => "Collectable",
            _ => "None"
        };
    }
}