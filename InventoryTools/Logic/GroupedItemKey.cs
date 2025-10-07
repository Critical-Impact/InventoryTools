using System;
using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

namespace InventoryTools.Logic;

public class GroupedItemKey : IEquatable<GroupedItemKey>
{
    public uint ItemId { get; set; }

    public uint? WorldId { get; set; }

    public bool? IsHq { get; set; }

    public bool? IsCollectable { get; set; }

    public ulong? OwnerId { get; set; }

    public ulong? CharacterId { get; set; }

    public bool IsGrouped =>
        this.WorldId != null || this.IsHq != null || this.OwnerId != null || this.CharacterId != null || this.IsCollectable != null;

    public static GroupedItemKey FromInventoryItem(
        InventoryItem item,
        GroupedItemGroup groupedItemGroup,
        ICharacterMonitor characterMonitor,
        Dictionary<ulong, ulong?> characterRetainerMap)
    {
        var key = new GroupedItemKey
        {
            ItemId = item.ItemId
        };
        if (groupedItemGroup.HasFlag(GroupedItemGroup.None))
        {
            key.ItemId = item.ItemId;
        }

        if (groupedItemGroup.HasFlag(GroupedItemGroup.World))
        {
            key.WorldId = characterMonitor.GetCharacterById(item.RetainerId)?.WorldId ?? null;
        }

        if (groupedItemGroup.HasFlag(GroupedItemGroup.Owner))
        {
            key.OwnerId = characterMonitor.GetCharacterById(item.RetainerId)?.OwnerId ?? null;
        }

        if (groupedItemGroup.HasFlag(GroupedItemGroup.IsCollectable))
        {
            key.IsCollectable = item.IsCollectible;
        }

        if (groupedItemGroup.HasFlag(GroupedItemGroup.IsHq))
        {
            key.IsHq = item.IsHQ;
        }

        if (groupedItemGroup.HasFlag(GroupedItemGroup.Character))
        {
            key.CharacterId = item.RetainerId;
        }

        return key;
    }

    public bool Equals(GroupedItemKey? other)
    {
        return this.ItemId == other?.ItemId && this.WorldId == other?.WorldId &&
               this.IsHq == other?.IsHq && this.OwnerId == other?.OwnerId && this.CharacterId == other?.CharacterId && this.IsCollectable == other?.IsCollectable;
    }

    public override bool Equals(object? obj)
    {
        return obj is GroupedItemKey key && this.ItemId == key.ItemId && this.WorldId == key.WorldId &&
               this.IsHq == key.IsHq && this.OwnerId == key.OwnerId && this.CharacterId == key.CharacterId && this.IsCollectable == key.IsCollectable;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.ItemId, this.WorldId, this.IsHq, this.OwnerId, this.CharacterId, this.IsCollectable);
    }
}
