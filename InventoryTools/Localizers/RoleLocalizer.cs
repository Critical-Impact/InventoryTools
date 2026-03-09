using AllaganLib.GameSheets.Sheets.Rows;

namespace InventoryTools.Localizers;

public class RoleLocalizer : ILocalizer<RoleType>
{
    public string Format(RoleType instance)
    {
        switch (instance)
        {
            case RoleType.Tank:
                return "Tank";
            case RoleType.DPSMelee:
                return "DPS (Melee)";
            case RoleType.DPSRanged:
                return "DPS (Ranged)";
            case RoleType.Healer:
                return "Healer";
            case RoleType.Crafting:
                return "Crafting";
            case RoleType.Gathering:
                return "Gathering";
            case RoleType.Other:
                return "Other";
        }

        return "Unknown";
    }
}