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
            character.CharacterId = 100 + (ulong)CharacterCount;
            return character;
        }

        public static Character GenerateRetainer(Character owner)
        {
            RetainerCount++;
            var character = new Character();
            character.Name = "Retainer " + RetainerCount; 
            character.OwnerId = owner.CharacterId;
            character.CharacterId = 300 + (ulong)RetainerCount;
            return character;
        }
        
        public static Inventory GenerateBlankInventory(Character character)
        {
            var newInventory = new Inventory(character.CharacterType, character.CharacterId);
            newInventory.FillSlots();
            return newInventory;
        }
        
        public static void FillInventory(Inventory inventory, InventoryCategory category, uint itemId, uint quantity)
        {
            var inventoryItems = inventory.GetItemsByCategory(category);
            foreach (var item in inventoryItems)
            {
                var newItem = GenerateItem(inventory.CharacterId, item.SortedContainer, (short)item.SortedSlotIndex, itemId, quantity);
                inventory.AddItem(newItem);
            }
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

        public static InventoryItem GenerateItem(ulong characterId, InventoryType type, short slot, uint itemId, uint quantity)
        {
            var inventoryItem = new InventoryItem();
            inventoryItem.Slot = slot;
            inventoryItem.ItemId = itemId;
            inventoryItem.SortedSlotIndex = slot;
            inventoryItem.SortedContainer = type;
            inventoryItem.SortedCategory = type.ToInventoryCategory();
            inventoryItem.RetainerId = characterId;
            inventoryItem.Quantity = quantity;
            return inventoryItem;
        }
    }
}