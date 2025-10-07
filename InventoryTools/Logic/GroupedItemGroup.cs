using System;

namespace InventoryTools.Logic;

[Flags]
public enum GroupedItemGroup
{
    None = 0,
    IsCollectable = 1,
    World = 2,
    IsHq = 4,
    Owner = 8,
    Character = 16,
}
