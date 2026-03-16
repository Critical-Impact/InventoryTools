using System.Collections.Generic;
using AllaganLib.GameSheets.Model;

namespace InventoryTools.Compendium.Sections.Options;

public record ItemListSectionOptions : ViewSectionOptions
{
    public required IEnumerable<ItemInfo> Items { get; init; }
    public bool ShowTryOn { get; init; } //maybe make into menu system
}