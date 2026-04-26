using System;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Localizers;

public class RelicWeaponTypeLocalizer : ILocalizer<RelicWeaponType>
{
    public string Format(RelicWeaponType relicWeaponType)
    {
        switch (relicWeaponType)
        {
            case RelicWeaponType.Unknown:
                return "Unknown";
            case RelicWeaponType.ZodiacBase:
                return "Base";
            case RelicWeaponType.ZodiacZenith:
                return "Zenith";
            case RelicWeaponType.ZodiacAtma:
                return "Atma";
            case RelicWeaponType.ZodiacAnimus:
                return "Animus";
            case RelicWeaponType.ZodiacNovus:
                return "Novus";
            case RelicWeaponType.ZodiacNexus:
                return "Nexus";
            case RelicWeaponType.ZodiacZodiac:
                return "Zodiac";
            case RelicWeaponType.ZodiacZeta:
                return "Zeta";
            case RelicWeaponType.AnimaAnimated:
                return "Animated";
            case RelicWeaponType.AnimaAwoken:
                return "Awoken";
            case RelicWeaponType.AnimaAnima:
                return "Anima";
            case RelicWeaponType.AnimaHyperconductive:
                return "Hyperconductive";
            case RelicWeaponType.AnimaReconditioned:
                return "Reconditioned";
            case RelicWeaponType.AnimaSharpened:
                return "Sharpened";
            case RelicWeaponType.AnimaComplete:
                return "Complete";
            case RelicWeaponType.AnimaLux:
                return "Lux";
            case RelicWeaponType.EurekanAntiquated:
                return "Antiquated";
            case RelicWeaponType.EurekanBase:
                return "Base";
            case RelicWeaponType.EurekanBase1:
                return "Base + 1";
            case RelicWeaponType.EurekanBase2:
                return "Base + 2";
            case RelicWeaponType.EurekanAnemos:
                return "Anemos";
            case RelicWeaponType.EurekanPagos:
                return "Pagos";
            case RelicWeaponType.EurekanPagos1:
                return "Pagos + 1";
            case RelicWeaponType.EurekanElemental:
                return "Elemental";
            case RelicWeaponType.EurekanElemental1:
                return "Elemental + 1";
            case RelicWeaponType.EurekanElemental2:
                return "Elemental + 2";
            case RelicWeaponType.EurekanPyros:
                return "Pyros";
            case RelicWeaponType.EurekanHydatos:
                return "Hydatos";
            case RelicWeaponType.EurekanHydatos1:
                return "Hydatos + 1";
            case RelicWeaponType.EurekanBaseEureka:
                return "Base";
            case RelicWeaponType.EurekanEureka:
                return "Eureka";
            case RelicWeaponType.EurekanPhyseos:
                return "Physeos";
            case RelicWeaponType.ResistanceResistance:
                return "Resistance";
            case RelicWeaponType.ResistanceAugmentedResistance:
                return "Augmented Resistance";
            case RelicWeaponType.ResistanceRecollection:
                return "Recollection";
            case RelicWeaponType.ResistanceLawsOrder:
                return "Laws Order";
            case RelicWeaponType.ResistanceAugmentedLawsOrder:
                return "Augmented Laws Order";
            case RelicWeaponType.ResistanceBlades:
                return "Blades";
            case RelicWeaponType.MandervilleManderville:
                return "Manderville";
            case RelicWeaponType.MandervilleAmazing:
                return "Amazing";
            case RelicWeaponType.MandervilleMajestic:
                return "Majestic";
            case RelicWeaponType.MandervilleMandervillous:
                return  "Mandervillous";
            case RelicWeaponType.PhantomPenumbrae:
                return "Penumbrae";
            case RelicWeaponType.PhantomUmbrae:
                return "Umbrae";
            case RelicWeaponType.PhantomObscurum:
                return "Obscurum";
        }

        return relicWeaponType.ToString();
    }
}