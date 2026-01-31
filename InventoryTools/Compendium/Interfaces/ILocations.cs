using System.Collections.Generic;
using AllaganLib.GameSheets.Model;

namespace InventoryTools.Compendium.Interfaces;

public interface ILocations
{
    public List<NamedLocation>? GetLocations(uint rowId);
}

public class NamedLocation
{
    public ILocation Location { get; set; }
    public string Name { get; set; }
    public string? MapLinkName { get; set; }

    public NamedLocation(ILocation location, string name, string? mapLinkName = null)
    {
        Location = location;
        Name = name;
        MapLinkName = mapLinkName;
    }
}