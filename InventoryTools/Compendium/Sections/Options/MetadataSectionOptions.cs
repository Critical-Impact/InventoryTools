using System;
using System.Collections.Generic;

namespace InventoryTools.Compendium.Sections.Options;

public sealed class MetadataSectionOptions
{
    public required string SectionName { get; init; }

    public required List<Row> Rows { get; init; }

    public sealed class Row
    {
        public required string Label { get; init; }
        public required Func<string?> Value { get; init; }
        public Func<bool>? ShouldDraw { get; init; }
    }
}