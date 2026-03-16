using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Sections.Options;

public sealed class MapLinkViewSectionOptions
{
    public string SectionName { get; init; } = "Location";
    public MapLinkEntry? MapLink { get; init; }
}