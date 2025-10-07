namespace InventoryTools.Logic;

public class GroupedItem(GroupedItemKey grouping)
{
    public GroupedItemKey Grouping { get; set; } = grouping;

    public uint ItemId { get; set; }

    public uint? WorldId { get; set; }

    public bool? IsHq { get; set; }

    public bool? IsCollectable { get; set; }

    public ulong? OwnerId { get; set; }

    public ulong? RetainerId { get; set; }

    public uint Quantity { get; set; }
}
