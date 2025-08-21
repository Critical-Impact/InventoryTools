using System.Collections.Generic;
using System.Linq;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Debuggers;

public class InventoryScannerDebuggerPane : IDebugPane
{
    private readonly InventoryScanner _inventoryScanner;

    public InventoryScannerDebuggerPane(InventoryScanner inventoryScanner)
    {
        _inventoryScanner = inventoryScanner;
    }
    public string Name => "Inventory Scanner";
    public void Draw()
    {
        ImGui.TextUnformatted("Inventories Seen via Network Traffic");
        foreach (var inventory in _inventoryScanner.LoadedInventories)
        {
            ImGui.TextUnformatted(inventory.ToString());
        }

        ImGui.TextUnformatted("Retainer Inventories Seen via Network Traffic");
        foreach (var inventory in _inventoryScanner.InMemoryRetainers)
        {
            ImGui.TextUnformatted(inventory.Key.ToString());
            foreach (var hashSet in inventory.Value)
            {
                ImGui.TextUnformatted(hashSet.ToString());
            }
        }
        if (ImGui.TreeNode("Character Bags 1##characterBags1"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterBag1.Length; i++)
            {
                var item = _inventoryScanner.CharacterBag1[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Bags 2##characterBags2"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterBag2.Length; i++)
            {
                var item = _inventoryScanner.CharacterBag2[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Bags 3##characterBags3"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterBag3.Length; i++)
            {
                var item = _inventoryScanner.CharacterBag3[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Bags 4##characterBags4"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterBag4.Length; i++)
            {
                var item = _inventoryScanner.CharacterBag4[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Equipped##characterEquipped"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterEquipped.Length; i++)
            {
                var item = _inventoryScanner.CharacterEquipped[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Crystals##characterCrystals"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterCrystals.Length; i++)
            {
                var item = _inventoryScanner.CharacterCrystals[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Character Currency##characterCurrency"))
        {
            for (int i = 0; i < _inventoryScanner.CharacterCurrency.Length; i++)
            {
                var item = _inventoryScanner.CharacterCurrency[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Saddlebag Left##saddlebagLeft"))
        {
            for (int i = 0; i < _inventoryScanner.SaddleBag1.Length; i++)
            {
                var item = _inventoryScanner.SaddleBag1[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Saddlebag Right##saddlebagRight"))
        {
            for (int i = 0; i < _inventoryScanner.SaddleBag2.Length; i++)
            {
                var item = _inventoryScanner.SaddleBag2[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Premium Saddlebag Left##premiumSaddleBagLeft"))
        {
            for (int i = 0; i < _inventoryScanner.PremiumSaddleBag1.Length; i++)
            {
                var item = _inventoryScanner.PremiumSaddleBag1[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Premium Saddlebag Right##premiumSaddleBagRight"))
        {
            for (int i = 0; i < _inventoryScanner.PremiumSaddleBag2.Length; i++)
            {
                var item = _inventoryScanner.PremiumSaddleBag2[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Head##armouryHead"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryHead.Length; i++)
            {
                var item = _inventoryScanner.ArmouryHead[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - MainHand##armouryMainHand"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryMainHand.Length; i++)
            {
                var item = _inventoryScanner.ArmouryMainHand[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Body##armouryBody"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryBody.Length; i++)
            {
                var item = _inventoryScanner.ArmouryBody[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Hands##armouryHands"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryHands.Length; i++)
            {
                var item = _inventoryScanner.ArmouryHands[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Legs##armouryLegs"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryLegs.Length; i++)
            {
                var item = _inventoryScanner.ArmouryLegs[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Feet##armouryFeet"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryFeet.Length; i++)
            {
                var item = _inventoryScanner.ArmouryFeet[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Off Hand##armouryOffHand"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryOffHand.Length; i++)
            {
                var item = _inventoryScanner.ArmouryOffHand[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Ears##armouryEars"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryEars.Length; i++)
            {
                var item = _inventoryScanner.ArmouryEars[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Neck##armouryNeck"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryNeck.Length; i++)
            {
                var item = _inventoryScanner.ArmouryNeck[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Wrists##armouryWrists"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryWrists.Length; i++)
            {
                var item = _inventoryScanner.ArmouryWrists[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - Rings##armouryRings"))
        {
            for (int i = 0; i < _inventoryScanner.ArmouryRings.Length; i++)
            {
                var item = _inventoryScanner.ArmouryRings[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoury - SoulCrystals##armourySoulCrystals"))
        {
            for (int i = 0; i < _inventoryScanner.ArmourySoulCrystals.Length; i++)
            {
                var item = _inventoryScanner.ArmourySoulCrystals[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Chest 1##freeCompanyBags1"))
        {
            for (int i = 0; i < _inventoryScanner.FreeCompanyBag1.Length; i++)
            {
                var item = _inventoryScanner.FreeCompanyBag1[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Chest 2##freeCompanyBags2"))
        {
            for (int i = 0; i < _inventoryScanner.FreeCompanyBag2.Length; i++)
            {
                var item = _inventoryScanner.FreeCompanyBag2[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Chest 3##freeCompanyBags3"))
        {
            for (int i = 0; i < _inventoryScanner.FreeCompanyBag3.Length; i++)
            {
                var item = _inventoryScanner.FreeCompanyBag3[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Chest 4##freeCompanyBags4"))
        {
            for (int i = 0; i < _inventoryScanner.FreeCompanyBag4.Length; i++)
            {
                var item = _inventoryScanner.FreeCompanyBag4[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Chest 5##freeCompanyBags5"))
        {
            for (int i = 0; i < _inventoryScanner.FreeCompanyBag5.Length; i++)
            {
                var item = _inventoryScanner.FreeCompanyBag5[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Free Company Currency##freeCompanyCurrency"))
        {
            var bagType = (InventoryType)CriticalCommonLib.Enums.InventoryType.FreeCompanyCurrency;
            var bag = _inventoryScanner.GetInventoryByType(bagType);
            var bagLoaded = _inventoryScanner.IsBagLoaded(bagType);
            if (ImGui.TreeNode(bagType.ToString() + (bagLoaded ? " (Loaded)" : " (Not Loaded)")))
            {
                var itemCount = bag.Count(c => c.ItemId != 0);
                ImGui.Text(itemCount + "/" + bag.Length);
                for (int i = 0; i < bag.Length; i++)
                {
                    var item = bag[i];
                    Utils.PrintOutObject(item, (ulong)i, new List<string>());
                }

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Armoire##armoire"))
        {
            for (int i = 0; i < _inventoryScanner.Armoire.Length; i++)
            {
                var item = _inventoryScanner.Armoire[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Glamour Chest##glamourChest"))
        {
            for (int i = 0; i < _inventoryScanner.GlamourChest.Length; i++)
            {
                var item = _inventoryScanner.GlamourChest[i];
                Utils.PrintOutObject(item, (ulong)i, new List<string>());
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Bag 1##retainerBag1"))
        {
            foreach (var retainer in _inventoryScanner.RetainerBag1)
            {
                if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##1" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Bag 2##retainerBag2"))
        {
            foreach (var retainer in _inventoryScanner.RetainerBag2)
            {
                if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##2" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Bag 3##retainerBag3"))
        {
            foreach (var retainer in _inventoryScanner.RetainerBag3)
            {
                if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##3" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Bag 4##retainerBag4"))
        {
            foreach (var retainer in _inventoryScanner.RetainerBag4)
            {
                if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##4" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }
            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Bag 5##retainerBag5"))
        {
            foreach (var retainer in _inventoryScanner.RetainerBag5)
            {
                if (ImGui.TreeNode("Retainer Bag " + retainer.Key + "##5" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Equipped##retainerEquipped"))
        {
            foreach (var retainer in _inventoryScanner.RetainerEquipped)
            {
                if (ImGui.TreeNode("Retainer Equipped" + retainer.Key + "##equipped" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Market##retainerMarket"))
        {
            foreach (var retainer in _inventoryScanner.RetainerMarket)
            {
                if (ImGui.TreeNode("Retainer Market" + retainer.Key + "##market" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Market Prices##retainerMarketPrices"))
        {
            foreach (var retainer in _inventoryScanner.RetainerMarketPrices)
            {
                if (ImGui.TreeNode("Retainer Market" + retainer.Key + "##market" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Crystals##retainerCrystals"))
        {
            foreach (var retainer in _inventoryScanner.RetainerCrystals)
            {
                if (ImGui.TreeNode("Retainer Crystals" + retainer.Key + "##crystals" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Retainer Gil##retainerGil"))
        {
            foreach (var retainer in _inventoryScanner.RetainerGil)
            {
                if (ImGui.TreeNode("Retainer Gil" + retainer.Key + "##gil" + retainer.Key))
                {
                    for (int i = 0; i < retainer.Value.Length; i++)
                    {
                        var item = retainer.Value[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }
        if (ImGui.TreeNode("Gearsets##gearsets"))
        {
            foreach (var gearSet in _inventoryScanner.GetGearSets())
            {
                ImGui.Text(gearSet.Key + ":");
                foreach (var actualset in gearSet.Value)
                {
                    ImGui.Text(actualset.Item1 + " : " + actualset.Item2);
                }
            }

            ImGui.TreePop();
        }
        var bags = new[]
        {
            InventoryType.HousingInteriorPlacedItems1,
            InventoryType.HousingInteriorPlacedItems2,
            InventoryType.HousingInteriorPlacedItems3,
            InventoryType.HousingInteriorPlacedItems4,
            InventoryType.HousingInteriorPlacedItems5,
            InventoryType.HousingInteriorPlacedItems6,
            InventoryType.HousingInteriorPlacedItems7,
            InventoryType.HousingInteriorPlacedItems8,
            InventoryType.HousingInteriorStoreroom1,
            InventoryType.HousingInteriorStoreroom2,
            InventoryType.HousingInteriorStoreroom3,
            InventoryType.HousingInteriorStoreroom4,
            InventoryType.HousingInteriorStoreroom5,
            InventoryType.HousingInteriorStoreroom6,
            InventoryType.HousingInteriorStoreroom7,
            InventoryType.HousingInteriorStoreroom8,
            InventoryType.HousingExteriorAppearance,
            InventoryType.HousingInteriorAppearance,
            InventoryType.HousingExteriorPlacedItems,
            InventoryType.HousingExteriorStoreroom,
        };

        if (ImGui.TreeNode("Housing Inventories"))
        {
            foreach (var bagType in bags)
            {
                var bag = _inventoryScanner.GetInventoryByType(bagType);
                var bagLoaded = _inventoryScanner.IsBagLoaded(bagType);
                if (ImGui.TreeNode(bagType.ToString() + (bagLoaded ? " (Loaded)" : " (Not Loaded)")))
                {
                    var itemCount = bag.Count(c => c.ItemId != 0);
                    ImGui.Text(itemCount + "/" + bag.Length);
                    for (int i = 0; i < bag.Length; i++)
                    {
                        var item = bag[i];
                        Utils.PrintOutObject(item, (ulong)i, new List<string>());
                    }

                    ImGui.TreePop();
                }

            }
            ImGui.TreePop();
        }
    }
}