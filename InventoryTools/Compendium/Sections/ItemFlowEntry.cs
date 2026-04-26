using AllaganLib.GameSheets.Sheets.Rows;

namespace InventoryTools.Compendium.Sections;

public class ItemFlowEntry
{
    public required ItemRow Item { get; init; }
    public required ItemRow? Item2 { get; init; }
    public required string Title { get; init; }
}