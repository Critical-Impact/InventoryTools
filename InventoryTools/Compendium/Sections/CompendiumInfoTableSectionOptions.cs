using System.Collections.Generic;

namespace InventoryTools.Compendium.Sections;

public record CompendiumInfoTableSectionOptions : CompendiumViewSectionOptions
{
    public required IEnumerable<(string Header, string Value, bool IsVisible)> Items { get; init; }
}