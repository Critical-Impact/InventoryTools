using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.IPC;
using InventoryToolsTesting.Services;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests
{
    [TestFixture]
    public class IpcItemCountTests : BaseTest
    {
        private Character _character = null!;
        private Character _otherCharacter = null!;
        private Character _retainer = null!;
        private Character _freeCompany = null!;
        private uint _itemId;

        private static uint[] Categories(params InventoryCategory[] categories)
        {
            var converted = new uint[categories.Length];
            for (var index = 0; index < categories.Length; index++)
            {
                converted[index] = (uint)categories[index];
            }

            return converted;
        }

        [SetUp]
        public void Init()
        {
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();

            //Rye flour, just cause
            _itemId = itemSheet.GetRow(4825)!.RowId;

            _character = GenerateCharacter();
            _otherCharacter = GenerateCharacter();
            _retainer = GenerateRetainer(_character);
            _freeCompany = GenerateFreeCompany(_character);

            characterMonitor.LoadExistingRetainers(new Dictionary<ulong, Character>
            {
                { _character.CharacterId, _character },
                { _otherCharacter.CharacterId, _otherCharacter },
                { _retainer.CharacterId, _retainer },
                { _freeCompany.CharacterId, _freeCompany },
            });
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);

            foreach (var inventory in inventoryMonitor.Inventories)
            {
                inventoryMonitor.ClearCharacterInventories(inventory.Key);
            }

            var characterInventory = GenerateBlankInventory(_character);
            var otherCharacterInventory = GenerateBlankInventory(_otherCharacter);
            var retainerInventory = GenerateBlankInventory(_retainer);
            var freeCompanyInventory = GenerateBlankInventory(_freeCompany);
            inventoryMonitor.AddInventory(new List<Inventory> { characterInventory, otherCharacterInventory, retainerInventory, freeCompanyInventory });

            characterInventory.AddItem(GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, _itemId, 5));
            characterInventory.AddItem(GenerateItem(_character.CharacterId, InventoryType.SaddleBag0, 0, _itemId, 2));
            retainerInventory.AddItem(GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, _itemId, 3));
            freeCompanyInventory.AddItem(GenerateItem(_freeCompany.CharacterId, InventoryType.FreeCompanyBag0, 0, _itemId, 7));
            otherCharacterInventory.AddItem(GenerateItem(_otherCharacter.CharacterId, InventoryType.Bag0, 0, _itemId,
                11));

            inventoryMonitor.LoadExistingData(new List<CriticalCommonLib.Models.InventoryItem>());
        }

        private IPCService GetIpcService()
        {
            return Host.Services.GetRequiredService<IPCService>()!;
        }

        [Test]
        public void TestItemCountOwnedByCategoryCountsRetainers()
        {
            var ipcService = GetIpcService();

            Assert.AreEqual(10, ipcService.ItemCountOwnedByCategory(_itemId, true, [], false));

            Assert.AreEqual(17, ipcService.ItemCountOwnedByCategory(_itemId, true, [], true));

            Assert.AreEqual(21, ipcService.ItemCountOwnedByCategory(_itemId, false, [], false));
            Assert.AreEqual(28, ipcService.ItemCountOwnedByCategory(_itemId, false, [], true));
        }

        [Test]
        public void TestItemCountOwnedByCategoryFiltersCategories()
        {
            var ipcService = GetIpcService();

            Assert.AreEqual(5, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.CharacterBags), false));
            Assert.AreEqual(2, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.CharacterSaddleBags), false));
            Assert.AreEqual(3, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.RetainerBags), false));
            Assert.AreEqual(8, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.CharacterBags, InventoryCategory.RetainerBags), false));

            Assert.AreEqual(0, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.Armoire), false));
        }

        [Test]
        public void TestSharedStorageFlagOverridesCategories()
        {
            var ipcService = GetIpcService();

            Assert.AreEqual(0, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.FreeCompanyBags), false));
            Assert.AreEqual(7, ipcService.ItemCountOwnedByCategory(_itemId, true, Categories(InventoryCategory.FreeCompanyBags), true));
        }

        [Test]
        public void TestGetItemCountsByCharacter()
        {
            var ipcService = GetIpcService();

            var owned = ipcService.GetItemCountsByCharacter(_itemId, true, [], false);
            Assert.AreEqual(2, owned.Count);
            Assert.AreEqual(7, owned[_character.CharacterId]);
            Assert.AreEqual(3, owned[_retainer.CharacterId]);
            Assert.IsFalse(owned.ContainsKey(_freeCompany.CharacterId));
            Assert.IsFalse(owned.ContainsKey(_otherCharacter.CharacterId));

            var ownedWithShared = ipcService.GetItemCountsByCharacter(_itemId, true, [], true);
            Assert.AreEqual(3, ownedWithShared.Count);
            Assert.AreEqual(7, ownedWithShared[_freeCompany.CharacterId]);

            var missing = ipcService.GetItemCountsByCharacter(_itemId, true, Categories(InventoryCategory.RetainerBags), false);
            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(3, missing[_retainer.CharacterId]);
        }

        [Test]
        public void TestGetCharacterItemsHandlesUnknownCharacter()
        {
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            Assert.IsFalse(inventoryMonitor.Inventories.ContainsKey(12345));

            Assert.AreEqual(0, GetIpcService().GetCharacterItems(12345).Count);
            Assert.AreEqual(0, GetIpcService().GetCharacterItemsByType(12345, (uint)InventoryType.Bag0).Count);
        }
    }
}