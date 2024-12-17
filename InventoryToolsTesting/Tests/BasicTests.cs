using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryToolsTesting.Services;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests
{
    [TestFixture]
    public class BasicTests : BaseTest
    {
        private Character _character = null!;
        private Character _character2 = null!;
        private Character _retainer = null!;
        private Character _retainer2 = null!;

        [SetUp]
        public void SetupTests()
        {
            var configuration = Host.Services.GetRequiredService<InventoryToolsConfiguration>()!;
            configuration.DisplayCrossCharacter = false;
        }

        [SetUp]
        public void Init()
        {
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            _character = Fixtures.GenerateCharacter();
            _character2 = Fixtures.GenerateCharacter();
            _retainer = Fixtures.GenerateRetainer(_character);
            _retainer2 = Fixtures.GenerateRetainer(_character);
            var characters = new Dictionary<ulong, Character>();
            characters.Add(_character.CharacterId, _character);
            characters.Add(_retainer.CharacterId, _retainer);
            characters.Add(_retainer2.CharacterId, _retainer2);
            characters.Add(_character2.CharacterId, _character2);
            characterMonitor.LoadExistingRetainers(characters);
        }
        [Test]
        public void TestSearchFilter()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.SearchFilter;

            characterMonitor.OverrideActiveCharacter(_character.CharacterId);
            var inventory = Fixtures.GenerateBlankInventory(_character);
            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, 1000, 1));
            inventoryMonitor.AddInventory(inventory);

            var oneList = listFilterService.RefreshList(searchFilter);
            Assert.AreEqual( 1, oneList.Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

        }

        [Test]
        public void TestSortFilter()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.DestinationAllRetainers = true;
            searchFilter.FilterType = FilterType.SortingFilter;

            //Flour, just cause
            var ryeFlour = excelCache.GetItemSheet().GetRow(4825)!;
            var wheatFlour = excelCache.GetItemSheet().GetRow(4826)!;
            var cinnamon = excelCache.GetItemSheet().GetRow(4828)!;


            characterMonitor.OverrideActiveRetainer(_retainer.CharacterId);
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);

            var inventory = Fixtures.GenerateBlankInventory(_character);
            var retainerInventory = Fixtures.GenerateBlankInventory(_retainer);
            var retainerInventory2 = Fixtures.GenerateBlankInventory(_retainer2);
            var inventories = new List<Inventory>() { inventory, retainerInventory, retainerInventory2 };
            inventoryMonitor.AddInventory(inventories);

            //Nothing to sort
            var emptyList = listFilterService.RefreshList(searchFilter);
            Assert.True(emptyList.All(c => c.InventoryItem != null && c.InventoryItem.IsEmpty));

            //1 item to retainer
            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            var oneList = listFilterService.RefreshList( searchFilter);
            Assert.AreEqual( 1, oneList.Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Duplicates only, 0 items to retainer
            searchFilter.DuplicatesOnly = true;
            Assert.True(listFilterService.RefreshList(searchFilter).All(c => c.InventoryItem != null && c.InventoryItem.IsEmpty));

            //Duplicates only, 1 item to retainer
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            Assert.AreEqual(1, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Duplicates only, 1 item to retainer, add a unrelated item
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 1, wheatFlour.RowId, 1));
            Assert.AreEqual(1,listFilterService.RefreshList( searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Duplicates only, max out item in existing inventory
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.Base.StackSize));
            var generateFilteredList = listFilterService.RefreshList( searchFilter);
            Assert.AreEqual(1, generateFilteredList.Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Duplicates only, max out item in existing inventory then spill over
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.Base.StackSize - 1));
            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 2));
            Assert.AreEqual(2, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Duplicates only, max out retainer, should go nowhere, boy got some cinnamon, 2 items in inventory
            Fixtures.FillInventory(retainerInventory, InventoryCategory.RetainerBags, cinnamon.RowId, cinnamon.Base.StackSize);
            Assert.AreEqual(0,listFilterService.RefreshList( searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Allow item to spill over to retainer 2, but we are in retainer 1 so nothing shows up
            searchFilter.DuplicatesOnly = false;
            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Only;
            Assert.AreEqual(0, listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null && !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer2.CharacterId));


            //Allow item to spill over to retainer 2 for real
            characterMonitor.OverrideActiveRetainer(0);
            Assert.AreEqual(1, listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer2.CharacterId));

            //Item should goto 2nd retainer first
            Fixtures.FillInventory(retainerInventory, InventoryCategory.RetainerBags, 0, 0);
            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            retainerInventory2.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.Base.StackSize - 1));
            Assert.True(listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer2.CharacterId) == 1);

            //Filter items when in specific retainer, should show 0 sorted items as we are in the first retainer and not the 2nd
            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            characterMonitor.OverrideActiveRetainer(_retainer.CharacterId);
            Assert.True(listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer2.CharacterId) == 0);
            Assert.True(listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer.CharacterId) == 0);

            //Switch to retainer 2
            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            characterMonitor.OverrideActiveRetainer(_retainer2.CharacterId);
            Assert.True(listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer2.CharacterId) == 1);
            Assert.True(listFilterService.RefreshList( searchFilter).Count(c =>c.InventoryItem != null && c.SortingResult != null &&  !c.InventoryItem.IsEmpty && c.SortingResult.DestinationRetainerId == _retainer.CharacterId) == 0);
        }
        //
        [Test]
        public void TestDestinationInventory()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.SortingFilter;

            //Flour, just cause
            var ryeFlour = excelCache.GetItemSheet().GetRow(4825)!;
            var wheatFlour = excelCache.GetItemSheet().GetRow(4826)!;
            var cinnamon = excelCache.GetItemSheet().GetRow(4828)!;


            searchFilter.DestinationInventories = new List<(ulong, InventoryCategory)>() {(_character.CharacterId, InventoryCategory.CharacterSaddleBags)};
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);

            var inventory = Fixtures.GenerateBlankInventory(_character);
            var inventories = new List<Inventory>() { inventory };
            inventoryMonitor.AddInventory(inventories);

            //Nothing to sort
            var emptyList = listFilterService.RefreshList(searchFilter);
            Assert.True(emptyList.All(c => c.InventoryItem != null && c.InventoryItem.IsEmpty));

            //1 item to retainer
            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            var oneList = listFilterService.RefreshList(searchFilter);
            Assert.AreEqual( 1, oneList.Count(c => c.InventoryItem != null &&  !c.InventoryItem.IsEmpty));

        }

        [Test]
        public void TestCraftItemFilter()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.GameItemFilter;
            searchFilter.IntegerFilters = new Dictionary<string, int>() {{"CraftItemFilter", 32224}};

            var emptyList = listFilterService.RefreshList( searchFilter);
            Assert.AreEqual(16, emptyList.Count);
        }

        [Test]
        public void TestDuplicates()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.SourceAllRetainers = true;
            searchFilter.DestinationAllRetainers = true;
            searchFilter.FilterType = FilterType.SortingFilter;
            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;

            //Flour, just cause
            var ryeFlour = excelCache.GetItemSheet().GetRow(4825)!;
            var wheatFlour = excelCache.GetItemSheet().GetRow(4826)!;
            var cinnamon = excelCache.GetItemSheet().GetRow(4828)!;

            characterMonitor.OverrideActiveRetainer(_retainer.CharacterId);
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);


            var inventory = Fixtures.GenerateBlankInventory(_character);
            var retainerInventory = Fixtures.GenerateBlankInventory(_retainer);
            var retainerInventory2 = Fixtures.GenerateBlankInventory(_retainer2);

            var inventories = new List<Inventory>() { inventory, retainerInventory, retainerInventory2 };
            inventoryMonitor.AddInventory(inventories);

            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            retainerInventory2.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));

            //1 in player bag, 1 in retainer bag 1 as retainer 1 is active
            Assert.AreEqual(2, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null &&  !c.InventoryItem.IsEmpty));

            characterMonitor.OverrideActiveRetainer(0);
            //1 in player bag, 1 in retainer bag 1, 1 in retainer bag 2 as no retainer is active
            Assert.AreEqual(3, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null &&  !c.InventoryItem.IsEmpty));

            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;

            //1 from bag -> retainer 1
            //1 from retainer 1 -> retainer 2
            //1 from retainer 2 -> retainer 1
            var actual = listFilterService.RefreshList(searchFilter).Where(c => c.InventoryItem != null && c.SortingResult != null && !c.InventoryItem.IsEmpty).ToList();
            Assert.AreEqual(_retainer.CharacterId, actual.ToList()[0].SortingResult!.DestinationRetainerId);
            Assert.AreEqual(_retainer2.CharacterId, actual.ToList()[1].SortingResult!.DestinationRetainerId);
            Assert.AreEqual(_retainer.CharacterId, actual.ToList()[2].SortingResult!.DestinationRetainerId);
        }

        [Test]
        public void TestCrossCharacterSearching()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;
            var configuration = Host.Services.GetRequiredService < InventoryToolsConfiguration>();

            var searchFilter = new FilterConfiguration();
            searchFilter.SourceCategories = new HashSet<InventoryCategory>()
            {
                InventoryCategory.RetainerBags, InventoryCategory.CharacterBags
            };
            searchFilter.FilterType = FilterType.SearchFilter;

            //Flour, just cause
            var ryeFlour = excelCache.GetItemSheet().GetRow(4825)!;
            var wheatFlour = excelCache.GetItemSheet().GetRow(4826)!;
            var cinnamon = excelCache.GetItemSheet().GetRow(4828)!;

            characterMonitor.OverrideActiveRetainer(_retainer.CharacterId);
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);

            var inventory = Fixtures.GenerateBlankInventory(_character);
            var retainerInventory = Fixtures.GenerateBlankInventory(_retainer);
            var retainerInventory2 = Fixtures.GenerateBlankInventory(_retainer2);
            var characterInventory2 = Fixtures.GenerateBlankInventory(_character2);
            var inventories = new List<Inventory>()
                { inventory, retainerInventory, retainerInventory2, characterInventory2 };

            inventoryMonitor.AddInventory(inventories);

            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            retainerInventory2.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            characterInventory2.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));

            //Cross character off, should pick up 3
            Assert.AreEqual(3, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Cross character on, should pick up 4
            configuration.DisplayCrossCharacter = true;
            Assert.AreEqual(4, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Test cross character source filter setting
            configuration.DisplayCrossCharacter = false;
            searchFilter.SourceIncludeCrossCharacter = true;
            Assert.AreEqual(4, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Test cross character destination filter setting
            searchFilter.DestinationIncludeCrossCharacter = true;
            searchFilter.FilterType = FilterType.SortingFilter;
            searchFilter.DestinationCategories = new HashSet<InventoryCategory>()
            {
                InventoryCategory.CharacterBags
            };
            searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            //With a active retainer and filter items in retainers set to yes, only 1 item shows up
            var resultSortedItems = listFilterService.RefreshList(searchFilter);
            Assert.AreEqual(1, resultSortedItems.Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Without a active retainer
            characterMonitor.OverrideActiveRetainer(0);
            Assert.AreEqual(4, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));


            //Test cross character destination filter setting override
            searchFilter.SourceCategories = new HashSet<InventoryCategory>()
            {
                InventoryCategory.CharacterBags
            };
            configuration.DisplayCrossCharacter = true;
            searchFilter.DestinationIncludeCrossCharacter = false;
            searchFilter.SourceIncludeCrossCharacter = true;

            Assert.AreEqual(1, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));
        }

        [Test]
        public void TestInventoryCategoryFilters()
        {
            var listFilterService = Host.Services.GetRequiredService<ListFilterService>()!;
            var characterMonitor = Host.Services.GetRequiredService<ICharacterMonitor>()!;
            var inventoryMonitor = Host.Services.GetRequiredService<TestInventoryMonitor>()!;
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;

            var searchFilter = new FilterConfiguration();
            searchFilter.FilterType = FilterType.SearchFilter;
            searchFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};

            var sortFilter = new FilterConfiguration();
            sortFilter.FilterType = FilterType.SortingFilter;
            sortFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            sortFilter.DestinationCategories = new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};

            //Flour, just cause
            var ryeFlour = excelCache.GetItemSheet().GetRow(4825)!;
            var wheatFlour = excelCache.GetItemSheet().GetRow(4826)!;
            var cinnamon = excelCache.GetItemSheet().GetRow(4828)!;

            characterMonitor.OverrideActiveRetainer(_retainer.CharacterId);
            characterMonitor.OverrideActiveCharacter(_character.CharacterId);


            var inventory = Fixtures.GenerateBlankInventory(_character);
            var retainerInventory = Fixtures.GenerateBlankInventory(_retainer);
            var retainerInventory2 = Fixtures.GenerateBlankInventory(_retainer2);
            var characterInventory2 = Fixtures.GenerateBlankInventory(_character2);
            var inventories = new List<Inventory>()
                { inventory, retainerInventory, retainerInventory2, characterInventory2 };

            inventoryMonitor.AddInventory(inventories);


            inventory.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));
            retainerInventory.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            retainerInventory2.AddItem(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1));
            characterInventory2.AddItem(Fixtures.GenerateItem(_character.CharacterId, InventoryType.Bag0, 0, ryeFlour.RowId, 1));

            //Just character bags as source
            Assert.AreEqual(1, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Add retainer bags as source
            searchFilter.SourceCategories.Add(InventoryCategory.RetainerBags);
            Assert.AreEqual(3, listFilterService.RefreshList(searchFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));

            //Sort filter, character to retainer bags
            Assert.AreEqual(1, listFilterService.RefreshList(sortFilter).Count(c => c.InventoryItem != null && !c.InventoryItem.IsEmpty));
        }

        [Test]
        public void TestMarketOrdering()
        {
            var retainerMarket = new List<InventoryItem>();

            //pactmaker's garden scythe
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 0, 36707, 1));
            //super potion
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 1, 23167, 1));

            //honey muffin
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 2, 4698, 1));
            //bubble chocolate
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 3, 4735, 1));
            //tsai tou vounou
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 4, 36060, 1));
            //giant pumpkin
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 5, 36100, 1));

            //quickarm materia X
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 6, 33941, 1));
            //thavnarian onion
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 7, 8166, 1));
            //folded futon
            retainerMarket.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 8, 28976, 1));

            retainerMarket = retainerMarket.SortByRetainerMarketOrder()
                .ToList();
            for (var index = 0; index < retainerMarket.Count; index++)
            {
                var inventoryItem = retainerMarket[index];
                Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
            }

            var retainerMarket2 = new List<InventoryItem>();

            //sailor brais
            retainerMarket2.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 0, 7537, 1));
            //allagan aetherstone - weapon
            retainerMarket2.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 1, 15098, 1));

            //eastern teahouse bench
            retainerMarket2.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 2, 17983, 1));

            //apparel showcase
            retainerMarket2.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 3, 32234, 1));

            //factory beam
            retainerMarket2.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 4, 30403, 1));


            retainerMarket2 = retainerMarket2.SortByRetainerMarketOrder()
                .ToList();
            for (var index = 0; index < retainerMarket2.Count; index++)
            {
                var inventoryItem = retainerMarket2[index];
                Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
            }

            var retainerMarket3 = new List<InventoryItem>();

            //crystarium stove
            retainerMarket3.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 0, 27284, 1));

            //necklace display stand
            retainerMarket3.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 1, 28149, 1));

            //restaurant showcase
            retainerMarket3.Add(Fixtures.GenerateItem(_retainer.CharacterId, InventoryType.RetainerMarket, 2, 35579, 1));

            retainerMarket3 = retainerMarket3.SortByRetainerMarketOrder()
                .ToList();
            for (var index = 0; index < retainerMarket3.Count; index++)
            {
                var inventoryItem = retainerMarket3[index];
                Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
            }
        }

        [Test]
        public void TestCompanyCraftRequirements()
        {
            var excelCache = Host.Services.GetRequiredService<ExcelCache>()!;
            var itemRow = excelCache.GetItemSheet().GetRow(10157);
            var itemCompanyCraftResultSources = itemRow.GetSourcesByType<ItemCompanyCraftResultSource>(ItemInfoType.FreeCompanyCraftRecipe);
            Assert.IsTrue(itemCompanyCraftResultSources.Count != 0);
            var craftItems = itemRow.CompanyCraftSequence!.MaterialsRequired(null);
            Assert.AreEqual(craftItems.Count, 42);
        }

        [Test]
        public void TestGarlandToolsRegex()
        {
            Regex regex = new(@".*Gather: (\d [^.]*). Craft: (\d [^.]*)");
            Match match = regex.Match("Bluespirit Alembic. Gather: 3 Stiperstone, 12 Bluespirit Ore, 4 Manasilver Sand, Silver Ore. Craft: 3 Bluespirit Tile, Manasilver Nugget.");
            if (match.Success)
            {
                PluginLog.Info(match.Captures.Count.ToString());
                if (match.Groups.Count == 3)
                {
                    PluginLog.Info("matched with 3 groups");
                    var gather = match.Groups[1].Value;
                    var craft = match.Groups[2].Value;

                    gather = Regex.Replace(gather, @"[\d-]", string.Empty);
                    craft = Regex.Replace(craft, @"[\d-]", string.Empty);

                    var gatherItems = gather.Split(", ");
                    var craftItems = craft.Split(", ");
                    var list = gatherItems.Select(c => "=" + c.Trim()).ToList();
                    list.AddRange(craftItems.Select(c => "=" + c.Trim()).ToList());
                    var recipe = String.Join("||", list);

                }
            }
        }
    }
}