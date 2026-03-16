using AllaganLib.GameSheets.Model;

namespace InventoryTools.Compendium.Models;

public sealed record MapLinkEntry(
    uint Icon,
    string Name,
    string Subtitle,
    ILocation Location
);