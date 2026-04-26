using System;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Localizers;

public class RelicWeaponCategoryLocalizer : ILocalizer<RelicWeaponCategory>
{
    public string Format(RelicWeaponCategory instance)
    {
        switch (instance)
        {
            case RelicWeaponCategory.Zodiac:
                return  "Zodiac Weapons";
            case RelicWeaponCategory.Anima:
                return  "Anima Weapons";
            case RelicWeaponCategory.Eurekan:
                return  "Eurekan Weapons";
            case RelicWeaponCategory.Resistance:
                return  "Resistance Weapons";
            case RelicWeaponCategory.Manderville:
                return  "Manderville Weapons";
            case RelicWeaponCategory.Phantom:
                return  "Phantom Weapons";
        }
        return instance.ToString();
    }
}