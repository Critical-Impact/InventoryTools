using System.Collections.Generic;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Sections.Options;

public record class MapLinksViewSectionOptions : SectionOptions
{
    public IReadOnlyList<MapLinkEntry>? MapLinks { get; init; }
}