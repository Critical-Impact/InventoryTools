using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;

namespace InventoryTools.Compendium.Types.Extra;

public enum CofferCategory
{
    WeaponCoffers,
    JobArms,
    ArmourCoffers,
    AccessoryCoffers,
    GearSets,
    TreasureMaps,
    TalesOfAdventure,
    Miscellaneous,
    Coffers,
    CardPacks,
    Logograms,
    AncientRecords,
}

/// <summary>
/// A container item (coffer, card pack, logogram, etc.) and the items it can give.
/// </summary>
public class CofferContents
{
    /// <summary>
    /// The item ID of the container.
    /// </summary>
    public required uint RowId { get; init; }

    public required ItemRow Item { get; init; }

    /// <summary>
    /// The source type of the container.
    /// </summary>
    public required ItemInfoType SourceType { get; init; }

    /// <summary>
    /// The category of the container.
    /// </summary>
    public required CofferCategory Category { get; init; }

    /// <summary>
    /// The items the container can give.
    /// </summary>
    public required IReadOnlyList<ItemInfo> Contents { get; init; }
}
