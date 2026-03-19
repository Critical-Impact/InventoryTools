using AllaganLib.GameSheets.Sheets.Rows;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types.Extra;

public enum ChocoboItemSourceType
{
    BuddyItem,
    BuddyEquip,
}

public struct ChocoboItem
{
    public uint RowId { get; init; }

    public ItemRow Item { get; init; }

    public RowRef<BuddyItem>? BuddyItem { get; init; }

    public RowRef<BuddyEquip>? BuddyEquip { get; init; }

    public ChocoboItemSourceType SourceType { get; init; }
}