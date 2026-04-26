using System.Collections.Generic;

namespace InventoryTools.Compendium.Sections.Options;

public record ItemFlowSectionOptions : SectionOptions
{
    public required IReadOnlyList<ItemFlowEntry> Items { get; init; }
    public int ItemsPerColumn { get; set; } = 3;
}