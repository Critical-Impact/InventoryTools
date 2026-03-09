using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Sections;

public sealed class MapLinkViewSectionOptions
{
    public string SectionName { get; init; } = "Location";
    public MapLinkEntry? MapLink { get; init; }
}