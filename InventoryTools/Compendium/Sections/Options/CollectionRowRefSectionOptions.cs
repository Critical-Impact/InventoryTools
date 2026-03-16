using System;
using System.Collections.Generic;
using Lumina.Excel;

namespace InventoryTools.Compendium.Sections.Options;

public sealed class CollectionRowRefSectionOptions
{
    public required string SectionName { get; init; }
    public required List<RowRef> RelatedRefs { get; init; }
    public Type? Filter { get; init; } = null;
    public bool HideIfEmpty { get; init; } = true;
}