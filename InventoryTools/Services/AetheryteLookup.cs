using System.Numerics;
using AllaganLib.GameSheets.Sheets.Helpers;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Services;

public class AetheryteLookup
{
    public RowRef<Map> Map { get; }
    public RowRef<Aetheryte> Aetheryte { get; }
    public SubrowRef<MapMarker> MapMarkerCollection { get; }
    public int SubrowIndex { get; }

    public MapMarker MapMarker => MapMarkerCollection.Value[SubrowIndex];

    public float DistanceToAetheryte(Vector2 mapPosition)
    {
        var aetherytePosition = new Vector2(MapUtility.MarkerToMap(MapMarker.X, Map.Value.SizeFactor) / 100f, MapUtility.MarkerToMap(MapMarker.Y, Map.Value.SizeFactor) / 100f);
        return Vector2.Distance(aetherytePosition, mapPosition);
    }

    public AetheryteLookup(RowRef<Map> map, RowRef<Aetheryte> aetheryte, SubrowRef<MapMarker> mapMarkerCollection, int subrowIndex)
    {
        Map = map;
        Aetheryte = aetheryte;
        MapMarkerCollection = mapMarkerCollection;
        SubrowIndex = subrowIndex;
    }
}