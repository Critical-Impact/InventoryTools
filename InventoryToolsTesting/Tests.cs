using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Logging;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using NUnit.Framework;

namespace InventoryToolsTesting
{
    [TestFixture]
    public class Tests
    {
        private Character? _character;
        private Character? _character2;
        private Character? _retainer;
        private Character? _retainer2;
        private CharacterMonitor? _characterMonitor;
        private PluginLogic? _pluginLogic;

        [OneTimeSetUp]
        public void Steup()
        {
            var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
            Service.ExcelCache = new ExcelCache(lumina);
        }
        
        [SetUp]
        public void Init()
        {
            _characterMonitor = new CharacterMonitor(true);
            _pluginLogic = new PluginLogic(true);
            PluginService.InitialiseTesting(_characterMonitor, _pluginLogic);
            ConfigurationManager.Config = new InventoryToolsConfiguration();
            _character = Fixtures.GenerateCharacter();
            _character2 = Fixtures.GenerateCharacter();
            _retainer = Fixtures.GenerateRetainer(_character);
            _retainer2 = Fixtures.GenerateRetainer(_character);
            var characters = new Dictionary<ulong, Character>();
            characters.Add(_character.CharacterId, _character);
            characters.Add(_retainer.CharacterId, _retainer);
            characters.Add(_retainer2.CharacterId, _retainer2);
            characters.Add(_character2.CharacterId, _character2);
            _characterMonitor.LoadExistingRetainers(characters);
        }

        [SetUp]
        public void SetupTests()
        {
            ConfigurationManager.Config.DisplayCrossCharacter = false;
        }
        
        [Test]
        public void TestSearchFilter()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.SearchFilter;
            
            if (_character != null && _retainer != null)
            {
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);
                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                inventories.Add(_character.CharacterId, categoryDictionary);
                
                var emptyList = searchFilter.GenerateFilteredList(inventories).Result;
                Assert.True(emptyList.SortedItems.All(c => c.InventoryItem.IsEmpty));

                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, 1000, 1);
                
                var oneList = searchFilter.GenerateFilteredList(inventories).Result;
                Assert.AreEqual( 1, oneList.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
            }
        }

        
        [Test]
        public void TestSortFilter()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.DestinationAllRetainers = true;
            searchFilter.FilterType = FilterType.SortingFilter;
            
            //Flour, just cause
            var ryeFlour = Service.ExcelCache.GetItemExSheet().GetRow(4825);
            var wheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(4826);
            var cinnamon = Service.ExcelCache.GetItemExSheet().GetRow(4828);
            
            
            if (_character != null && _retainer != null && _retainer2 != null && ryeFlour != null && wheatFlour != null && cinnamon != null)
            {
                _characterMonitor?.OverrideActiveRetainer(_retainer.CharacterId);
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);

                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();

                
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);   
                
                var retainerInventory = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer);
                var retainerCategoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary.Add(InventoryCategory.RetainerBags, retainerInventory);
                
                var retainerInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer2);
                var retainerCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary2.Add(InventoryCategory.RetainerBags, retainerInventory2);
                
                inventories.Add(_character.CharacterId, categoryDictionary);
                inventories.Add(_retainer.CharacterId, retainerCategoryDictionary);
                inventories.Add(_retainer2.CharacterId, retainerCategoryDictionary2);
                
                //Nothing to sort
                var emptyList = searchFilter.GenerateFilteredList(inventories).Result;
                Assert.True(emptyList.SortedItems.All(c => c.InventoryItem.IsEmpty));

                //1 item to retainer
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                var oneList = searchFilter.GenerateFilteredList( inventories).Result;
                Assert.AreEqual( 1, oneList.SortedItems.Count(c => !c.InventoryItem.IsEmpty));

                //Duplicates only, 0 items to retainer
                searchFilter.DuplicatesOnly = true;
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.All(c => c.InventoryItem.IsEmpty));
                
                //Duplicates only, 1 item to retainer
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty) == 1);
                
                //Duplicates only, 1 item to retainer, add a unrelated item
                retainerInventory[1] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 1, wheatFlour.RowId, 1);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty) == 1);
                
                //Duplicates only, max out item in existing inventory
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.StackSize);
                var generateFilteredList = searchFilter.GenerateFilteredList( inventories);
                Assert.True(generateFilteredList.Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty) == 1);
                
                //Duplicates only, max out item in existing inventory then spill over
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.StackSize - 1);
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 2);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty) == 2);
                
                //Duplicates only, max out retainer, should go nowhere, boy got some cinnamon, 2 items in inventory
                Fixtures.FillInventory(retainerInventory, cinnamon.RowId, cinnamon.StackSize);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty) == 0);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.UnsortableItems.Count(c => !c.IsEmpty) == 0);
                
                //Allow item to spill over to retainer
                searchFilter.DuplicatesOnly = false;
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer2.CharacterId) == 1);
                
                //Item should goto 2nd retainer first
                Fixtures.FillInventory(retainerInventory, 0, 0);
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                retainerInventory2[0] = Fixtures.GenerateItem(_retainer2, InventoryType.RetainerBag0, 0, ryeFlour.RowId, ryeFlour.StackSize - 1);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer2.CharacterId) == 1);
                
                //Filter items when in specific retainer, should show 0 sorted items as we are in the first retainer and not the 2nd
                searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
                _characterMonitor?.OverrideActiveRetainer(_retainer.CharacterId);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer2.CharacterId) == 0);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer.CharacterId) == 0);
                
                //Switch to retainer 2
                searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
                _characterMonitor?.OverrideActiveRetainer(_retainer2.CharacterId);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer2.CharacterId) == 1);
                Assert.True(searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty && c.DestinationRetainerId == _retainer.CharacterId) == 0);
            }
        }
        
        [Test]
        public void TestDestinationInventory()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.SortingFilter;
            
            //Flour, just cause
            var ryeFlour = Service.ExcelCache.GetItemExSheet().GetRow(4825);
            var wheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(4826);
            var cinnamon = Service.ExcelCache.GetItemExSheet().GetRow(4828);
            
            
            if (_character != null && ryeFlour != null && wheatFlour != null && cinnamon != null)
            {
                searchFilter.DestinationInventories = new List<(ulong, InventoryCategory)>() {(_character.CharacterId, InventoryCategory.CharacterSaddleBags)};
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);

                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();

                
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);

                var saddleBag = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterSaddleBags, _character);
                categoryDictionary.Add(InventoryCategory.CharacterSaddleBags, saddleBag);

                inventories.Add(_character.CharacterId, categoryDictionary);
                
                //Nothing to sort
                var emptyList = searchFilter.GenerateFilteredList( inventories).Result;
                Assert.True(emptyList.SortedItems.All(c => c.InventoryItem.IsEmpty));

                //1 item to retainer
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                var oneList = searchFilter.GenerateFilteredList( inventories).Result;
                Assert.AreEqual( 1, oneList.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
            }
        }
        
        [Test]
        public void TestCraftItemFilter()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.FilterType = FilterType.GameItemFilter;
            searchFilter.IntegerFilters = new Dictionary<string, int>() {{"CraftItemFilter", 32224}};
            
            
            if (_character != null)
            {
                var emptyList = searchFilter.GenerateFilteredList( new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>()).Result;
                Assert.AreEqual(emptyList.AllItems.Count,16);
            }
        }
        
        [Test]
        public void TestDuplicates()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.SourceAllRetainers = true;
            searchFilter.DestinationAllRetainers = true;
            searchFilter.FilterType = FilterType.SortingFilter;
            
            //Flour, just cause
            var ryeFlour = Service.ExcelCache.GetItemExSheet().GetRow(4825);
            var wheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(4826);
            var cinnamon = Service.ExcelCache.GetItemExSheet().GetRow(4828);
            
            if (_character != null && _retainer != null && _retainer2 != null && ryeFlour != null && wheatFlour != null && cinnamon != null)
            {
                _characterMonitor?.OverrideActiveRetainer(_retainer.CharacterId);
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);

                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);   
                
                var retainerInventory = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer);
                var retainerCategoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary.Add(InventoryCategory.RetainerBags, retainerInventory);
                
                var retainerInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer2);
                var retainerCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary2.Add(InventoryCategory.RetainerBags, retainerInventory2);
                
                inventories.Add(_character.CharacterId, categoryDictionary);
                inventories.Add(_retainer.CharacterId, retainerCategoryDictionary);
                inventories.Add(_retainer2.CharacterId, retainerCategoryDictionary2);
                
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                retainerInventory2[0] = Fixtures.GenerateItem(_retainer2, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                
                //1 in player bag, 1 in retainer bag 1, 1 in retainer bag 2
                Assert.AreEqual(3, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                searchFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
                
                //1 from bag to retainer, 1 from retainer to other retainer
                var actual = searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Where(c => !c.InventoryItem.IsEmpty).ToList();
                Assert.AreEqual(_retainer.CharacterId, actual.ToList()[0].DestinationRetainerId);
                Assert.AreEqual(_retainer2.CharacterId, actual.ToList()[1].DestinationRetainerId);
            }
        }
        
        [Test]
        public void TestCrossCharacterSearching()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.SourceAllCharacters = true;
            searchFilter.SourceAllRetainers = true;
            searchFilter.FilterType = FilterType.SearchFilter;
            
            //Flour, just cause
            var ryeFlour = Service.ExcelCache.GetItemExSheet().GetRow(4825);
            var wheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(4826);
            var cinnamon = Service.ExcelCache.GetItemExSheet().GetRow(4828);
            
            if (_character != null && _retainer != null && _retainer2 != null && _character2 != null && ryeFlour != null && wheatFlour != null && cinnamon != null)
            {
                _characterMonitor?.OverrideActiveRetainer(_retainer.CharacterId);
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);

                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);   
                
                var retainerInventory = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer);
                var retainerCategoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary.Add(InventoryCategory.RetainerBags, retainerInventory);
                
                var retainerInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer2);
                var retainerCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary2.Add(InventoryCategory.RetainerBags, retainerInventory2);
                
                var characterInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character2);
                var characterCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                characterCategoryDictionary2.Add(InventoryCategory.CharacterBags, characterInventory2);
                
                inventories.Add(_character.CharacterId, categoryDictionary);
                inventories.Add(_retainer.CharacterId, retainerCategoryDictionary);
                inventories.Add(_retainer2.CharacterId, retainerCategoryDictionary2);
                inventories.Add(_character2.CharacterId, characterCategoryDictionary2);
                
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                retainerInventory2[0] = Fixtures.GenerateItem(_retainer2, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                characterInventory2[0] = Fixtures.GenerateItem(_character2, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                
                //Cross character off, should pick up 3
                Assert.AreEqual(3, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                //Cross character on, should pick up 4
                ConfigurationManager.Config.DisplayCrossCharacter = true;
                Assert.AreEqual(4, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                //Test cross character source filter setting
                ConfigurationManager.Config.DisplayCrossCharacter = false;
                searchFilter.SourceIncludeCrossCharacter = true;
                Assert.AreEqual(4, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                //Test cross character destination filter setting
                searchFilter.DestinationIncludeCrossCharacter = true;
                searchFilter.FilterType = FilterType.SortingFilter;
                searchFilter.DestinationAllCharacters = true;

                Assert.AreEqual(2, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                

                //Test cross character destination filter setting override
                searchFilter.SourceAllRetainers = false;
                ConfigurationManager.Config.DisplayCrossCharacter = true;
                searchFilter.DestinationIncludeCrossCharacter = false;
                searchFilter.SourceIncludeCrossCharacter = true;

                //Because character 2 isnt the active character it wont actually try to sort from character 2 to character 0, hence the 0
                Assert.AreEqual(0, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
            }
        }
        
        [Test]
        public void TestInventoryCategoryFilters()
        {
            var filterManager = new FilterService();
            
            var searchFilter = new FilterConfiguration();
            searchFilter.FilterType = FilterType.SearchFilter;
            searchFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            
            var sortFilter = new FilterConfiguration();
            sortFilter.FilterType = FilterType.SortingFilter;
            sortFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            sortFilter.DestinationCategories = new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
            
            //Flour, just cause
            var ryeFlour = Service.ExcelCache.GetItemExSheet().GetRow(4825);
            var wheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(4826);
            var cinnamon = Service.ExcelCache.GetItemExSheet().GetRow(4828);
            
            if (_character != null && _retainer != null && _retainer2 != null && _character2 != null && ryeFlour != null && wheatFlour != null && cinnamon != null)
            {
                _characterMonitor?.OverrideActiveRetainer(_retainer.CharacterId);
                _characterMonitor?.OverrideActiveCharacter(_character.CharacterId);

                var inventories = new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                
                var inventory = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character);
                var categoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                categoryDictionary.Add(InventoryCategory.CharacterBags, inventory);   
                
                var retainerInventory = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer);
                var retainerCategoryDictionary = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary.Add(InventoryCategory.RetainerBags, retainerInventory);
                
                var retainerInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.RetainerBags, _retainer2);
                var retainerCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                retainerCategoryDictionary2.Add(InventoryCategory.RetainerBags, retainerInventory2);
                
                var characterInventory2 = Fixtures.GenerateBlankInventory(InventoryCategory.CharacterBags, _character2);
                var characterCategoryDictionary2 = new Dictionary<InventoryCategory, List<InventoryItem>>();
                characterCategoryDictionary2.Add(InventoryCategory.CharacterBags, characterInventory2);
                
                inventories.Add(_character.CharacterId, categoryDictionary);
                inventories.Add(_retainer.CharacterId, retainerCategoryDictionary);
                inventories.Add(_retainer2.CharacterId, retainerCategoryDictionary2);
                inventories.Add(_character2.CharacterId, characterCategoryDictionary2);
                
                inventory[0] = Fixtures.GenerateItem(_character, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                retainerInventory[0] = Fixtures.GenerateItem(_retainer, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                retainerInventory2[0] = Fixtures.GenerateItem(_retainer2, InventoryType.RetainerBag0, 0, ryeFlour.RowId, 1);
                characterInventory2[0] = Fixtures.GenerateItem(_character2, InventoryType.Bag0, 0, ryeFlour.RowId, 1);
                
                //Just character bags as source
                Assert.AreEqual(1, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                //Add retainer bags as source
                searchFilter.SourceCategories.Add(InventoryCategory.RetainerBags);
                Assert.AreEqual(3, searchFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
                
                //Sort filter, character to retainer bags
                Assert.AreEqual(1, sortFilter.GenerateFilteredList( inventories).Result.SortedItems.Count(c => !c.InventoryItem.IsEmpty));
            }
        }

        [Test]
        public void TestMarketOrdering()
        {
            if (_retainer != null)
            {
                var retainerMarket = new List<InventoryItem>();
                
                //pactmaker's garden scythe
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 0, 36707, 1));
                //super potion
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 1, 23167, 1));
                
                //honey muffin
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 2, 4698, 1));
                //bubble chocolate
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 3, 4735, 1));
                //tsai tou vounou
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 4, 36060, 1));
                //giant pumpkin
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 5, 36100, 1));
                
                //quickarm materia X
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 6, 33941, 1));
                //thavnarian onion
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 7, 8166, 1));
                //folded futon
                retainerMarket.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 8, 28976, 1));

                retainerMarket = retainerMarket.SortByRetainerMarketOrder()
                    .ToList();
                for (var index = 0; index < retainerMarket.Count; index++)
                {
                    var inventoryItem = retainerMarket[index];
                    Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
                }
                
                var retainerMarket2 = new List<InventoryItem>();
                
                //sailor brais
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 0, 7537, 1));
                //allagan aetherstone - weapon
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 1, 15098, 1));
                
                //eastern teahouse bench
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 2, 17983, 1));
                
                //apparel showcase
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 3, 32234, 1));
                
                //tier 2 metal aquarium
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 4, 21842, 1));
                
                //factory beam
                retainerMarket2.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 5, 30403, 1));
                

                retainerMarket2 = retainerMarket2.SortByRetainerMarketOrder()
                    .ToList();
                for (var index = 0; index < retainerMarket2.Count; index++)
                {
                    var inventoryItem = retainerMarket2[index];
                    Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
                }
                
                var retainerMarket3 = new List<InventoryItem>();
                
                //crystarium stove
                retainerMarket3.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 0, 27284, 1));
                
                //necklace display stand
                retainerMarket3.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 1, 28149, 1));
                
                //restaurant showcase
                retainerMarket3.Add(Fixtures.GenerateItem(_retainer, InventoryType.RetainerMarket, 2, 35579, 1));

                retainerMarket3 = retainerMarket3.SortByRetainerMarketOrder()
                    .ToList();
                for (var index = 0; index < retainerMarket3.Count; index++)
                {
                    var inventoryItem = retainerMarket3[index];
                    Assert.AreEqual(inventoryItem.Slot, index, inventoryItem.FormattedName + " in wrong slot");
                }
            }
        }

        [Test]
        public void TestCompanyCraftRequirements()
        {
            Assert.IsTrue(Service.ExcelCache.IsCompanyCraft(10157));
            var item = Service.ExcelCache.GetItemExSheet().GetRow(10157);
            if (item != null)
            {
                var craftItems = item.GetFlattenedCraftItems(true, 1);
                Assert.AreEqual(craftItems.Count, 43);
            }
        }

        [Test]
        public void TestGarlandToolsRegex()
        {
            Regex regex = new(@".*Gather: (\d [^.]*). Craft: (\d [^.]*)");
            Match match = regex.Match("Bluespirit Alembic. Gather: 3 Stiperstone, 12 Bluespirit Ore, 4 Manasilver Sand, Silver Ore. Craft: 3 Bluespirit Tile, Manasilver Nugget.");
            if (match.Success)
            {
                PluginLog.Log(match.Captures.Count.ToString());
                if (match.Groups.Count == 3)
                {
                    PluginLog.Log("matched with 3 groups");
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