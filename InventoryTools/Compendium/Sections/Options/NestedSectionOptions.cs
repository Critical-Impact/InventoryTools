using System.Collections.Generic;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Sections.Options;

public record NestedSectionOptions : SectionOptions
{
    public required IReadOnlyList<ICompendiumViewSection> Sections { get; init; }
}
