using InventoryTools.Compendium.Types.Extra;

namespace InventoryTools.Localizers;

public class ChocoboItemSourceTypeLocalizer : ILocalizer<ChocoboItemSourceType>
{
    public string Format(ChocoboItemSourceType itemSourceType)
    {
        switch (itemSourceType)
        {
            case ChocoboItemSourceType.BuddyItem:
                return "Consumable";
            case ChocoboItemSourceType.BuddyEquip:
                return "Equipment";
        }

        return "Unknown";
    }
}