using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Sections.Options;

public record MapLinkViewSectionOptions : SectionOptions
{
    public MapLinkEntry? MapLink { get; init; }
}