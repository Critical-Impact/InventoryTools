using System.Threading;
using AllaganLib.GameSheets.Sheets;
using Autofac;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic;
using InventoryToolsTesting.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using InventoryItem = CriticalCommonLib.Models.InventoryItem;

namespace InventoryToolsTesting.Tests.Abstract
{
    public class BaseTest : IMediatorSubscriber
    {
        public BaseTest()
        {
            Host = new TestBoot().CreateHost();
            MediatorService = Host.Services.GetRequiredService<MediatorService>();
            MediatorService.StartAsync(new CancellationToken());
            PluginLog = Host.Services.GetRequiredService<IPluginLog>();
        }

        public IPluginLog PluginLog { get; set; }
        public IHost Host { get; set; }
        public MediatorService MediatorService { get; }

        public FilterConfiguration.Factory GetFilterConfigurationFactory()
        {
            return Host.Services.GetRequiredService<FilterConfiguration.Factory>();
        }

        public InventoryItem.Factory GetInventoryItemFactory()
        {
            return Host.Services.GetRequiredService<InventoryItem.Factory>();
        }

        public Character.Factory GetCharacterFactory()
        {
            return Host.Services.GetRequiredService<Character.Factory>();
        }

        public Inventory.Factory GetInventoryFactory()
        {
            return Host.Services.GetRequiredService<Inventory.Factory>();
        }

        public CraftList.Factory GetCraftListFactory()
        {
            return Host.Services.GetRequiredService<CraftList.Factory>();
        }

        public InventoryChange.FromGameItemFactory GetInventoryChangeFactory()
        {
            return Host.Services.GetRequiredService<InventoryChange.FromGameItemFactory>();
        }

        public uint GetItemIdByName(string name)
        {
            return Host.Services.GetRequiredService<ItemSheet>().ItemsByName.TryGetValue(name, out var rowId) ? rowId : 0;
        }

        public static int CharacterCount = 0;
        public static int RetainerCount = 0;
        public Character GenerateCharacter()
        {
            CharacterCount++;
            var character = GetCharacterFactory().Invoke();
            character.Name = "Character " + CharacterCount;
            character.OwnerId = 0;
            character.CharacterId = 100 + (ulong)CharacterCount;
            return character;
        }

        public Character GenerateRetainer(Character owner)
        {
            RetainerCount++;
            var character = GetCharacterFactory().Invoke();
            character.Name = "Retainer " + RetainerCount;
            character.OwnerId = owner.CharacterId;
            character.CharacterId = 300 + (ulong)RetainerCount;
            return character;
        }

        public Inventory GenerateBlankInventory(Character character)
        {
            var newInventory = GetInventoryFactory().Invoke(character.CharacterType, character.CharacterId);
            newInventory.FillSlots();
            return newInventory;
        }

        public void FillInventory(Inventory inventory, InventoryCategory category, uint itemId, uint quantity)
        {
            var inventoryItems = inventory.GetItemsByCategory(category);
            foreach (var item in inventoryItems)
            {
                var newItem = GetInventoryItemFactory().Invoke();
                newItem.FromInventoryItem(GenerateItem(inventory.CharacterId, item.SortedContainer, (short)item.SortedSlotIndex, itemId, quantity));
                inventory.AddItem(newItem);
            }
        }

        public InventoryItem GenerateBlankItem(Character character, CriticalCommonLib.Enums.InventoryType type, short slot)
        {
            var inventoryItem = GetInventoryItemFactory().Invoke();
            inventoryItem.Slot = slot;
            inventoryItem.ItemId = 0;
            inventoryItem.SortedSlotIndex = 0;
            inventoryItem.SortedContainer = type;
            inventoryItem.SortedCategory = type.ToInventoryCategory();
            inventoryItem.RetainerId = character.CharacterId;
            return inventoryItem;
        }

        public InventoryItem GenerateItem(ulong characterId, CriticalCommonLib.Enums.InventoryType type, short slot, uint itemId, uint quantity)
        {
            var inventoryItem = GetInventoryItemFactory().Invoke();
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