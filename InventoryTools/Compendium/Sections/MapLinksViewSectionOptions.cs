using System.Collections.Generic;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Sections;

public sealed class MapLinksViewSectionOptions
{
    public string SectionName { get; init; } = "Locations";
    public IReadOnlyList<MapLinkEntry>? MapLinks { get; init; }
}