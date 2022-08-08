using System.Collections.Generic;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

namespace InventoryToolsTesting
{
    public static class Fixtures
    {
        public static int CharacterCount = 0;
        public static int RetainerCount = 0;
        public static Character GenerateCharacter()
        {
            CharacterCount++;
            var character = new Character();
            character.Name = "Character " + CharacterCount; 
            character.OwnerId = 0;
            character.CharacterId = (ulong)CharacterCount;
            return character;
        }

        public static Character GenerateRetainer(Character owner)
        {
            RetainerCount++;
            var character = new Character();
            character.Name = "Retainer " + RetainerCount; 
            character.OwnerId = owner.CharacterId;
            character.CharacterId = 100 + (ulong)RetainerCount;
            return character;
        }
        
        public static List<InventoryItem> GenerateBlankInventory(InventoryCategory type,
            Character character)
        {
            var bags = new Dictionary<InventoryType, int>();
            if (type == InventoryCategory.CharacterBags)
            {
                bags.Add(InventoryType.Bag0, 25);
                bags.Add(InventoryType.Bag1, 25);
                bags.Add(InventoryType.Bag2, 25);
                bags.Add(InventoryType.Bag3, 25);
            }
            if (type == InventoryCategory.RetainerBags)
            {
                bags.Add(InventoryType.RetainerBag0, 25);
                bags.Add(InventoryType.RetainerBag1, 25);
                bags.Add(InventoryType.RetainerBag2, 25);
                bags.Add(InventoryType.RetainerBag3, 25);
                bags.Add(InventoryType.RetainerBag4, 25);
                bags.Add(InventoryType.RetainerBag5, 25);
                bags.Add(InventoryType.RetainerBag6, 25);
            }
            if (type == InventoryCategory.CharacterSaddleBags)
            {
                bags.Add(InventoryType.SaddleBag0, 75);
                bags.Add(InventoryType.SaddleBag1, 75);
            }

            if (type == InventoryCategory.CharacterPremiumSaddleBags)
            {
                bags.Add(InventoryType.PremiumSaddleBag0, 75);
                bags.Add(InventoryType.PremiumSaddleBag1, 75);
            }

            var finalItems = new List<InventoryItem>();
            foreach (var bag in bags)
            {
                for (int i = 0; i < bag.Value; i++)
                {
                    finalItems.Add(GenerateBlankItem(character, bag.Key, (short)i));
                }
            }

            return finalItems;
        }
        
        public static List<InventoryItem> FillInventory(List<InventoryItem> inventory, uint itemId, uint quantity)
        {
            foreach (var item in inventory)
            {
                item.ItemId = itemId;
                item.Quantity = quantity;
            }

            return inventory;
        }

        public static InventoryItem GenerateBlankItem(Character character, InventoryType type, short slot)
        {
            var inventoryItem = new InventoryItem();
            inventoryItem.Slot = slot;
            inventoryItem.ItemId = 0;
            inventoryItem.SortedSlotIndex = 0;
            inventoryItem.SortedContainer = type;
            inventoryItem.SortedCategory = type.ToInventoryCategory();
            inventoryItem.RetainerId = character.CharacterId;
            return inventoryItem;
        }

        public static InventoryItem GenerateItem(Character character, InventoryType type, short slot, uint itemId, uint quantity)
        {
            var inventoryItem = new InventoryItem();
            inventoryItem.Slot = slot;
            inventoryItem.ItemId = itemId;
            inventoryItem.SortedSlotIndex = 0;
            inventoryItem.SortedContainer = type;
            inventoryItem.SortedCategory = type.ToInventoryCategory();
            inventoryItem.RetainerId = character.CharacterId;
            inventoryItem.Quantity = quantity;
            return inventoryItem;
        }
    }
}