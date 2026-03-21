using System;
using System.Collections.Generic;

namespace InventoryTools.Compendium.Sections.Options;

public record MetadataSectionOptions : SectionOptions
{
    public required List<Row> Rows { get; init; }

    public record Row
    {
        public required string Label { get; init; }
        public required Func<string?> Value { get; init; }
        public Func<bool>? ShouldDraw { get; init; }
    }
}