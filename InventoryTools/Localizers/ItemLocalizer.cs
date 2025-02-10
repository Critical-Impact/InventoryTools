using System.Collections.Generic;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Localizers;

public class ItemLocalizer
{
    private readonly ExcelSheet<Addon> _addonSheet;
    private Dictionary<uint, string> _cabinetNames;

    public ItemLocalizer(ExcelSheet<Addon> addonSheet)
    {
        _addonSheet = addonSheet;
        _cabinetNames = new();
    }

    public string CabinetName(InventoryItem inventoryItem)
    {
        if (inventoryItem.SortedContainer != InventoryType.Armoire)
        {
            return "";
        }

        var cabinetCategory = inventoryItem.Item.CabinetCategory;
        if (cabinetCategory == null)
        {
            return "Unknown Cabinet";
        }

        if (_cabinetNames.TryGetValue(cabinetCategory.Base.Category.RowId, out string? cabinetName))
        {
            return cabinetName;
        }

        cabinetName = _addonSheet.GetRowOrDefault(cabinetCategory.Base.Category.RowId)?.Text.ExtractText() ??
                      "Addon Text Not Found";

        _cabinetNames[cabinetCategory.Base.Category.RowId] = cabinetName;

        return cabinetName;
    }

    public string ItemDescription(InventoryItem inventoryItem)
    {
        if (inventoryItem.IsEmpty)
        {
            return "Empty";
        }

        var _item = inventoryItem.Item.NameString.ToString();
        if (inventoryItem.IsHQ)
        {
            _item += " (HQ)";
        }
        else if (inventoryItem.IsCollectible)
        {
            _item += " (Collectible)";
        }
        else
        {
            _item += " (NQ)";
        }

        if (inventoryItem.SortedCategory == InventoryCategory.Currency)
        {
            _item += " - " + SortedContainerName(inventoryItem);
        }
        else
        {
            _item += " - " + SortedContainerName(inventoryItem) + " - " + (inventoryItem.SortedSlotIndex + 1);
        }


        return _item;
    }

    public string FormattedBagLocation(InventoryItem inventoryItem)
    {
        if (inventoryItem.SortedContainer is InventoryType.GlamourChest or InventoryType.Currency or InventoryType.RetainerGil or InventoryType.FreeCompanyGil or InventoryType.Crystal or InventoryType.RetainerCrystal)
        {
            return SortedContainerName(inventoryItem);
        }
        return SortedContainerName(inventoryItem) + " - " + (inventoryItem.SortedSlotIndex + 1);
    }

    public string SortedContainerName(InventoryItem inventoryItem)
    {
        if(inventoryItem.SortedContainer is InventoryType.Bag0 or InventoryType.RetainerBag0)
        {
            return "Bag 1";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag1 or InventoryType.RetainerBag1)
        {
            return "Bag 2";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag2 or InventoryType.RetainerBag2)
        {
            return "Bag 3";
        }
        if(inventoryItem.SortedContainer is InventoryType.Bag3 or InventoryType.RetainerBag3)
        {
            return "Bag 4";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerBag4)
        {
            return "Bag 5";
        }
        if(inventoryItem.SortedContainer is InventoryType.SaddleBag0)
        {
            return "Saddlebag Left";
        }
        if(inventoryItem.SortedContainer is InventoryType.SaddleBag1)
        {
            return "Saddlebag Right";
        }
        if(inventoryItem.SortedContainer is InventoryType.PremiumSaddleBag0)
        {
            return "Premium Saddlebag Left";
        }
        if(inventoryItem.SortedContainer is InventoryType.PremiumSaddleBag1)
        {
            return "Premium Saddlebag Right";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryBody)
        {
            return "Armory - Body";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryEar)
        {
            return "Armory - Ear";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryFeet)
        {
            return "Armory - Feet";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryHand)
        {
            return "Armory - Hand";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryHead)
        {
            return "Armory - Head";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryLegs)
        {
            return "Armory - Legs";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryMain)
        {
            return "Armory - Main";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryNeck)
        {
            return "Armory - Neck";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryOff)
        {
            return "Armory - Offhand";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryRing)
        {
            return "Armory - Ring";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryWaist)
        {
            return "Armory - Waist";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmoryWrist)
        {
            return "Armory - Wrist";
        }
        if(inventoryItem.SortedContainer is InventoryType.ArmorySoulCrystal)
        {
            return "Armory - Soul Crystal";
        }
        if(inventoryItem.SortedContainer is InventoryType.GearSet0)
        {
            return "Equipped Gear";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerEquippedGear)
        {
            return "Equipped Gear";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag0)
        {
            return "Free Company Chest - 1";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag1)
        {
            return "Free Company Chest - 2";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag2)
        {
            return "Free Company Chest - 3";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag3)
        {
            return "Free Company Chest - 4";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag4)
        {
            return "Free Company Chest - 5";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag5)
        {
            return "Free Company Chest - 6";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag6)
        {
            return "Free Company Chest - 7";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag7)
        {
            return "Free Company Chest - 8";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag8)
        {
            return "Free Company Chest - 9";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag9)
        {
            return "Free Company Chest - 10";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyBag10)
        {
            return "Free Company Chest - 11";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerMarket)
        {
            return "Market";
        }
        if(inventoryItem.SortedContainer is InventoryType.GlamourChest)
        {
            return "Glamour Chest";
        }
        if(inventoryItem.SortedContainer is InventoryType.Armoire)
        {
            return "Armoire - " + CabinetName(inventoryItem);
        }
        if(inventoryItem.SortedContainer is InventoryType.Currency)
        {
            return "Currency";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyGil)
        {
            return "Free Company - Gil";
        }
        if(inventoryItem.SortedContainer is InventoryType.RetainerGil)
        {
            return "Currency";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyCrystal)
        {
            return "Free Company - Crystals";
        }
        if(inventoryItem.SortedContainer is InventoryType.FreeCompanyCurrency)
        {
            return "Free Company - Currency";
        }
        if(inventoryItem.SortedContainer is InventoryType.Crystal or InventoryType.RetainerCrystal)
        {
            return "Crystals";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorAppearance)
        {
            return "Housing Exterior Appearance";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorAppearance)
        {
            return "Housing Interior Appearance";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorStoreroom)
        {
            return "Housing Exterior Storeroom";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorStoreroom1 or InventoryType.HousingInteriorStoreroom2 or InventoryType.HousingInteriorStoreroom2 or InventoryType.HousingInteriorStoreroom3 or InventoryType.HousingInteriorStoreroom4 or InventoryType.HousingInteriorStoreroom5 or InventoryType.HousingInteriorStoreroom6 or InventoryType.HousingInteriorStoreroom7 or InventoryType.HousingInteriorStoreroom8)
        {
            return "Housing Interior Storeroom";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingInteriorPlacedItems1 or InventoryType.HousingInteriorPlacedItems2 or InventoryType.HousingInteriorPlacedItems2 or InventoryType.HousingInteriorPlacedItems3 or InventoryType.HousingInteriorPlacedItems4 or InventoryType.HousingInteriorPlacedItems5 or InventoryType.HousingInteriorPlacedItems6 or InventoryType.HousingInteriorPlacedItems7 or InventoryType.HousingInteriorPlacedItems8)
        {
            return "Housing Interior Placed Items";
        }
        if(inventoryItem.SortedContainer is InventoryType.HousingExteriorPlacedItems)
        {
            return "Housing Exterior Placed Items";
        }

        return inventoryItem.SortedContainer.ToString();
    }
}