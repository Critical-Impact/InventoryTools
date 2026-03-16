using System.Collections.Generic;

namespace InventoryTools.Compendium.Sections.Options;

public record InfoTableSectionOptions : ViewSectionOptions
{
    public required IEnumerable<(string Header, string Value, bool IsVisible)> Items { get; init; }
}