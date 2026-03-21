using System.Collections.Generic;
using AllaganLib.GameSheets.ItemSources;

namespace InventoryTools.Compendium.Sections.Options;

public record ItemSourcesSectionOptions : SectionOptions
{
    public required List<ItemSource> Sources { get; init; }
}