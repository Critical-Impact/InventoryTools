using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;

namespace InventoryTools.Localizers;

public class CraftGroupingLocalizer
{
    private readonly CraftTypeSheet _craftTypeSheet;
    private readonly MapSheet _mapSheet;

    public CraftGroupingLocalizer(CraftTypeSheet craftTypeSheet, MapSheet mapSheet)
    {
        _craftTypeSheet = craftTypeSheet;
        _mapSheet = mapSheet;
    }
    public string FormattedName(CraftGrouping craftGrouping)
    {
        var name = craftGrouping.CraftGroupType.FormattedName();
        if (craftGrouping.Depth != null)
        {
            name = craftGrouping.Depth.Value.ConvertToOrdinal() + " Tier " + name;
        }

        if (craftGrouping.ClassJobId != null)
        {
            var classJob = _craftTypeSheet.GetRowOrDefault(craftGrouping.ClassJobId.Value);
            if (classJob != null)
            {
                name = classJob.FormattedName + " - " + name;
            }
        }

        if (craftGrouping.MapId != null)
        {
            var map = _mapSheet.GetRowOrDefault(craftGrouping.MapId.Value);
            if (map != null)
            {
                name = map.FormattedName;
            }
        }

        return name;
    }
}