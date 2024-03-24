using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using DalaMock.Dalamud;
using Dalamud.Plugin.Services;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryToolsMock;
using Lumina;
using NUnit.Framework;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryToolsTesting
{
    [TestFixture]
    public class InventoryHistoryTests
    {
        private PluginLogic _pluginLogic = null!;
        private ICharacterMonitor _characterMonitor = null!;
        private IInventoryMonitor _inventoryMonitor = null!;
        private InventoryHistory _inventoryHistory = null!;

        [OneTimeSetUp]
        public void Setup()
        {
            var lumina = new Lumina.GameData("C:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack",
                new LuminaOptions()
                {
                    PanicOnSheetChecksumMismatch = false
                });
            _excelCache = new ExcelCache(lumina);
            Service.Framework = new MockFramework();
        }

        [SetUp]
        public void Init()
        {
            _pluginLogic = new PluginLogic(true);
            _characterMonitor = new MockCharacterMonitor();
            Service.Framework = new MockFramework();
            _inventoryMonitor = new InventoryMonitor(_characterMonitor, new MockCraftMonitor(), new MockInventoryScanner(), Service.Framework);
            _inventoryHistory = new InventoryHistory(_inventoryMonitor);
            PluginService.InitaliseExplicit(new MockServices()
            {
                CharacterMonitor = _characterMonitor,
                InventoryMonitor = _inventoryMonitor,
                PluginLogic = _pluginLogic
            });
            ConfigurationManager.Config = new InventoryToolsConfiguration();
        }

        [SetUp]
        public void SetupTests()
        {
            Configuration.DisplayCrossCharacter = false;
        }

        [Test]
        public void TestMoveItem()
        {
            
            //We have a single ash cavalry bow in slot 0, we move it from there to slot 1, not on first load
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 1);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 0, 0);

                var from2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 0, 0);
                var to2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 1915, 1);

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(from2, to2, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                changes.Add(inventoryChange2);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                Assert.AreEqual(1, processedChanges.Count);
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(1, processedChange.ToItem!.Slot);
                Assert.AreEqual(InventoryChangeReason.Moved, processedChange.InventoryChangeReason);
            }

            //We have a single ash cavalry bow in slot 0, we move it from there to slot 1, not on first load, order reversed
            {
                var from2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 1);
                var to2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 0, 0);

                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 0, 0);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 1915, 1);

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(from2, to2, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                changes.Add(inventoryChange2);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                
                Assert.AreEqual(1, processedChanges.Count);
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(1, processedChange.ToItem!.Slot);
                Assert.AreEqual(InventoryChangeReason.Moved, processedChange.InventoryChangeReason);
            }

            //We have a single ironwood lumber in slot 0 and then get another ironwood lumber in the same slot
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 1);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 2);

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(0, processedChange.ToItem!.Slot);
                Assert.AreEqual(1, processedChange.FromItem!.Quantity);
                Assert.AreEqual(2, processedChange.ToItem!.Quantity);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChange.InventoryChangeReason);
            }

            //We have a single HQ ironwood lumber in slot 0 and then it becomes NQ
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 1);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 1);
                from1.Flags = InventoryItem.ItemFlags.HQ;

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(0, processedChange.ToItem!.Slot);
                Assert.AreEqual(InventoryItem.ItemFlags.HQ, processedChange.FromItem!.Flags);
                Assert.AreEqual(InventoryItem.ItemFlags.None, processedChange.ToItem!.Flags);
                Assert.AreEqual(InventoryChangeReason.FlagsChanged, processedChange.InventoryChangeReason);
            }

            //We have a single HQ ironwood lumber in slot 0 and hand it in
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 1);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 0, 0);

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(0, processedChange.ToItem!.Slot);
                Assert.AreEqual(36199, processedChange.FromItem!.ItemId);
                Assert.AreEqual(0, processedChange.ToItem!.ItemId);
                Assert.AreEqual(InventoryChangeReason.Removed, processedChange.InventoryChangeReason);
            }

            //We have a single empty slot in slot 0 and get a single HQ ironwood lumber
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 0, 0);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 36199, 1);

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(0, processedChange.ToItem!.Slot);
                Assert.AreEqual(0, processedChange.FromItem!.ItemId);
                Assert.AreEqual(36199, processedChange.ToItem!.ItemId);
                Assert.AreEqual(InventoryChangeReason.Added, processedChange.InventoryChangeReason);
            }
            
            //Currency -100 and equipped Item repair status going up
            {
                var from1 = Fixtures.GenerateItem(100, InventoryType.Currency, 0, 1, 200);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Currency, 0, 1, 100);
                var from2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 1);
                var to2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 1);

                from2.Condition = 2;
                to2.Condition = 100;

                var inventoryChange = new InventoryChange(from1, to1, InventoryType.Currency, false);
                var inventoryChange2 = new InventoryChange(from2, to2, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                changes.Add(inventoryChange2);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                var processedChange = processedChanges.First();
                var processedChange2 = processedChanges.Last();
                Assert.AreEqual(2, processedChanges.Count);
                
                Assert.AreEqual(0, processedChange.FromItem!.Slot);
                Assert.AreEqual(0, processedChange.ToItem!.Slot);
                Assert.AreEqual(1, processedChange.FromItem!.ItemId);
                Assert.AreEqual(1, processedChange.ToItem!.ItemId);
                
                Assert.AreEqual(0, processedChange2.FromItem!.Slot);
                Assert.AreEqual(0, processedChange2.ToItem!.Slot);
                Assert.AreEqual(1915, processedChange2.FromItem!.ItemId);
                Assert.AreEqual(1915, processedChange2.ToItem!.ItemId);
                
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChange.InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.ConditionChanged, processedChange2.InventoryChangeReason);
            }

            //Desynth a single item in a stack of 2, the desynth creates a new item in a separate slot and adds 2 sets of crystals to an existing crystal count
            {
                var desynthStackFrom = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 2);
                var desynthStackTo = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 1915, 1);
                var iceShardFrom = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 3, 5);
                var iceShardTo = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 3, 10);
                var fireShardFrom = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 0, 0);
                var fireShardTo = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 2, 5);


                var inventoryChange = new InventoryChange(desynthStackFrom, desynthStackTo, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(iceShardFrom, iceShardTo, InventoryType.Crystal, false);
                var inventoryChange3 = new InventoryChange(fireShardFrom, fireShardTo, InventoryType.Crystal, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                changes.Add(inventoryChange2);
                changes.Add(inventoryChange3);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                Assert.AreEqual(3, processedChanges.Count);
            }
            
            //Full bag of items, randomly move them, then see if they all match
            {
                var items = new List<CriticalCommonLib.Models.InventoryItem>();
                for (int i = 0; i < 35; i++)
                {
                    var item = Fixtures.GenerateItem(100, InventoryType.Bag0, (short)i, (uint)(1915 + i), 1);
                    items.Add(item);
                }

                var originalOrder = items.ToList();
                items.Shuffle();
                var newOrder = items;
                var changes = new List<InventoryChange>();
                for (int i = 0; i < 35; i++)
                {
                    var newItem = Fixtures.GenerateItem(100, InventoryType.Bag0, (short)i, newOrder[i].ItemId, 1);
                    var inventoryChange = new InventoryChange(originalOrder[i], newItem, InventoryType.Bag0, false);
                    changes.Add(inventoryChange);
                }
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                Assert.AreEqual(true, processedChanges.All(c => c.InventoryChangeReason == InventoryChangeReason.Moved));
            }
            
            //Need to add in a test that checks to see if X of an item was removed and X was added into the stack of another item, match them together
            {
                var stack1From = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 38930, 2);
                var stack1To = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 38930, 1);
                
                var stack2From = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 38930, 1);
                var stack2To = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 38930, 2);


                var inventoryChange = new InventoryChange(stack1From, stack1To, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(stack2From, stack2To, InventoryType.Bag0, false);
                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange);
                changes.Add(inventoryChange2);
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);
                Assert.AreEqual(1, processedChanges.Count);
            }
            
            //Test to simulate crafting
            //1 item in Bag0 has it's quantity reduce from 5 to 3
            //1 item in Bag0 has it's quantity reduce from 8 to 5
            //1 item in Crystals has it's quantity reduce from 200 to 195
            //1 item in Crystals has it's quantity reduce from 250 to 245
            //1 empty slot in Bag0 has it's item ID change into a new item ID
            {
                // Initial inventory state
                var initialStack1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 4824, 5);
                var initialStack2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 12888, 8);
                var initialStack3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 8, 200);
                var initialStack4 = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 13, 250);
                var initialEmptySlot = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 0, 0);

                // Expected final inventory state
                var finalStack1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 4824, 3);
                var finalStack2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 12888, 5);
                var finalStack3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 8, 195);
                var finalStack4 = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 13, 245);
                var finalEmptySlot = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 24279, 1);

                var inventoryChange1 = new InventoryChange(initialStack1, finalStack1, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(initialStack2, finalStack2, InventoryType.Bag0, false);
                var inventoryChange3 = new InventoryChange(initialStack3, finalStack3, InventoryType.Crystal, false);
                var inventoryChange4 = new InventoryChange(initialStack4, finalStack4, InventoryType.Crystal, false);
                var inventoryChange5 = new InventoryChange(initialEmptySlot, finalEmptySlot, InventoryType.Bag0, true);

                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange1);
                changes.Add(inventoryChange2);
                changes.Add(inventoryChange3);
                changes.Add(inventoryChange4);
                changes.Add(inventoryChange5);

                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);

                Assert.AreEqual(5, processedChanges.Count);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 4824).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 12888).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 8).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 13).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.Added, processedChanges.Single(c => c.FromItem!.ItemId == 0).InventoryChangeReason);
            }
            
            
            //Test to simulate crafting
            //1 item in Bag0 has it's quantity reduce from 5 to 0
            //1 item in Bag0 has it's quantity reduce from 8 to 0
            //1 item in Crystals has it's quantity reduce from 200 to 195
            //1 item in Crystals has it's quantity reduce from 250 to 245
            //1 empty slot in Bag0 has it's item ID change into a new item ID
            {
                // Initial inventory state
                var initialStack1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 4824, 5);
                var initialStack2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 12888, 8);
                var initialStack3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 8, 200);
                var initialStack4 = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 13, 250);
                var initialEmptySlot = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 0, 0);

                // Expected final inventory state
                var finalStack1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 0, 0, 0);
                var finalStack2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 1, 0, 0);
                var finalStack3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 0, 8, 195);
                var finalStack4 = Fixtures.GenerateItem(100, InventoryType.Crystal, 1, 13, 245);
                var finalEmptySlot = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 24279, 1);

                var inventoryChange1 = new InventoryChange(initialStack1, finalStack1, InventoryType.Bag0, false);
                var inventoryChange2 = new InventoryChange(initialStack2, finalStack2, InventoryType.Bag0, false);
                var inventoryChange3 = new InventoryChange(initialStack3, finalStack3, InventoryType.Crystal, false);
                var inventoryChange4 = new InventoryChange(initialStack4, finalStack4, InventoryType.Crystal, false);
                var inventoryChange5 = new InventoryChange(initialEmptySlot, finalEmptySlot, InventoryType.Bag0, true);

                var changes = new List<InventoryChange>();
                changes.Add(inventoryChange1);
                changes.Add(inventoryChange2);
                changes.Add(inventoryChange3);
                changes.Add(inventoryChange4);
                changes.Add(inventoryChange5);

                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);

                Assert.AreEqual(5, processedChanges.Count);
                Assert.AreEqual(InventoryChangeReason.Removed, processedChanges.Single(c => c.FromItem!.ItemId == 4824).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.Removed, processedChanges.Single(c => c.FromItem!.ItemId == 12888).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 8).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 13).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.Added, processedChanges.Single(c => c.FromItem!.ItemId == 0).InventoryChangeReason);
            }

            //Desynthesis Test
            {
                var from0 = Fixtures.GenerateItem(100, InventoryType.Bag2, 30, 5576, 2);
                var to0 = Fixtures.GenerateItem(100, InventoryType.Bag2, 30, 5576, 1);
                var change0 = new InventoryChange(from0, to0, InventoryType.Bag2, false);
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag2, 31, 0, 0);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag2, 31, 5068, 2);
                var change1 = new InventoryChange(from1, to1, InventoryType.Bag2, false);
                var from2 = Fixtures.GenerateItem(100, InventoryType.Crystal, 12, 14, 244);
                var to2 = Fixtures.GenerateItem(100, InventoryType.Crystal, 12, 14, 245);
                var change2 = new InventoryChange(from2, to2, InventoryType.Crystal, false);
                var from3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 14, 16, 620);
                var to3 = Fixtures.GenerateItem(100, InventoryType.Crystal, 14, 16, 621);
                var change3 = new InventoryChange(from3, to3, InventoryType.Crystal, false);
                
                var changes = new List<InventoryChange>();
                changes.Add(change0);
                changes.Add(change1);
                changes.Add(change2);
                changes.Add(change3);
                
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);

                Assert.AreEqual(4, processedChanges.Count);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 5576).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.Added, processedChanges.Single(c => c.ToItem!.ItemId == 5068).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 14).InventoryChangeReason);
                Assert.AreEqual(InventoryChangeReason.QuantityChanged, processedChanges.Single(c => c.FromItem!.ItemId == 16).InventoryChangeReason);
            }
            
            //Item movement test
            {
                var from0 = Fixtures.GenerateItem(100, InventoryType.Bag2, 21, 12230, 15);
                var to0 = Fixtures.GenerateItem(100, InventoryType.Bag2, 21, 12230, 14);
                var change0 = new InventoryChange(from0, to0, InventoryType.Bag2, false);
                
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag3, 0, 0, 0);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag3, 0, 12230, 1);
                var change1 = new InventoryChange(from1, to1, InventoryType.Bag3, false);
                
                var changes = new List<InventoryChange>();
                changes.Add(change0);
                changes.Add(change1);
                
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);

                Assert.AreEqual(1, processedChanges.Count);
                Assert.AreEqual(InventoryChangeReason.Moved, processedChanges.Single(c => c.FromItem!.ItemId == 12230).InventoryChangeReason);
            }
            
            //Whole inventory shift test
            {
                var from0 = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 17545, 1);
                var to0 = Fixtures.GenerateItem(100, InventoryType.Bag0, 2, 20806, 1);
                var change0 = new InventoryChange(from0, to0, InventoryType.Bag0, false);
                var from1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 3, 20806, 1);
                var to1 = Fixtures.GenerateItem(100, InventoryType.Bag0, 3, 17545, 1);
                var change1 = new InventoryChange(from1, to1, InventoryType.Bag0, false);
                var from2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 20, 0, 0);
                var to2 = Fixtures.GenerateItem(100, InventoryType.Bag0, 20, 23167, 1);
                var change2 = new InventoryChange(from2, to2, InventoryType.Bag0, false);
                var from3 = Fixtures.GenerateItem(100, InventoryType.Bag0, 21, 0, 0);
                var to3 = Fixtures.GenerateItem(100, InventoryType.Bag0, 21, 6141, 368);
                var change3 = new InventoryChange(from3, to3, InventoryType.Bag0, false);
                var from4 = Fixtures.GenerateItem(100, InventoryType.Bag0, 22, 0, 0);
                var to4 = Fixtures.GenerateItem(100, InventoryType.Bag0, 22, 12669, 116);
                var change4 = new InventoryChange(from4, to4, InventoryType.Bag0, false);
                var from5 = Fixtures.GenerateItem(100, InventoryType.Bag0, 23, 0, 0);
                var to5 = Fixtures.GenerateItem(100, InventoryType.Bag0, 23, 16911, 5);
                var change5 = new InventoryChange(from5, to5, InventoryType.Bag0, false);
                var from6 = Fixtures.GenerateItem(100, InventoryType.Bag0, 24, 0, 0);
                var to6 = Fixtures.GenerateItem(100, InventoryType.Bag0, 24, 19882, 4);
                var change6 = new InventoryChange(from6, to6, InventoryType.Bag0, false);
                var from7 = Fixtures.GenerateItem(100, InventoryType.Bag0, 25, 0, 0);
                var to7 = Fixtures.GenerateItem(100, InventoryType.Bag0, 25, 27959, 14);
                var change7 = new InventoryChange(from7, to7, InventoryType.Bag0, false);
                var from8 = Fixtures.GenerateItem(100, InventoryType.Bag0, 26, 0, 0);
                var to8 = Fixtures.GenerateItem(100, InventoryType.Bag0, 26, 36038, 1);
                var change8 = new InventoryChange(from8, to8, InventoryType.Bag0, false);
                var from9 = Fixtures.GenerateItem(100, InventoryType.Bag0, 27, 0, 0);
                var to9 = Fixtures.GenerateItem(100, InventoryType.Bag0, 27, 30481, 1);
                var change9 = new InventoryChange(from9, to9, InventoryType.Bag0, false);
                var from10 = Fixtures.GenerateItem(100, InventoryType.Bag0, 28, 0, 0);
                var to10 = Fixtures.GenerateItem(100, InventoryType.Bag0, 28, 27878, 1);
                var change10 = new InventoryChange(from10, to10, InventoryType.Bag0, false);
                var from11 = Fixtures.GenerateItem(100, InventoryType.Bag0, 29, 0, 0);
                var to11 = Fixtures.GenerateItem(100, InventoryType.Bag0, 29, 27870, 5);
                var change11 = new InventoryChange(from11, to11, InventoryType.Bag0, false);
                var from12 = Fixtures.GenerateItem(100, InventoryType.Bag0, 30, 0, 0);
                var to12 = Fixtures.GenerateItem(100, InventoryType.Bag0, 30, 27871, 2);
                var change12 = new InventoryChange(from12, to12, InventoryType.Bag0, false);
                var from13 = Fixtures.GenerateItem(100, InventoryType.Bag0, 31, 0, 0);
                var to13 = Fixtures.GenerateItem(100, InventoryType.Bag0, 31, 4678, 1);
                var change13 = new InventoryChange(from13, to13, InventoryType.Bag0, false);
                var from14 = Fixtures.GenerateItem(100, InventoryType.Bag0, 32, 0, 0);
                var to14 = Fixtures.GenerateItem(100, InventoryType.Bag0, 32, 4643, 3);
                var change14 = new InventoryChange(from14, to14, InventoryType.Bag0, false);
                var from15 = Fixtures.GenerateItem(100, InventoryType.Bag0, 33, 0, 0);
                var to15 = Fixtures.GenerateItem(100, InventoryType.Bag0, 33, 4682, 1);
                var change15 = new InventoryChange(from15, to15, InventoryType.Bag0, false);
                var from16 = Fixtures.GenerateItem(100, InventoryType.Bag0, 34, 0, 0);
                var to16 = Fixtures.GenerateItem(100, InventoryType.Bag0, 34, 13717, 2);
                var change16 = new InventoryChange(from16, to16, InventoryType.Bag0, false);
                var from17 = Fixtures.GenerateItem(100, InventoryType.Bag1, 0, 23167, 1);
                var to17 = Fixtures.GenerateItem(100, InventoryType.Bag1, 0, 5259, 2);
                var change17 = new InventoryChange(from17, to17, InventoryType.Bag1, false);
                var from18 = Fixtures.GenerateItem(100, InventoryType.Bag1, 1, 6141, 368);
                var to18 = Fixtures.GenerateItem(100, InventoryType.Bag1, 1, 27698, 28);
                var change18 = new InventoryChange(from18, to18, InventoryType.Bag1, false);
                var from19 = Fixtures.GenerateItem(100, InventoryType.Bag1, 2, 12669, 116);
                var to19 = Fixtures.GenerateItem(100, InventoryType.Bag1, 2, 27701, 30);
                var change19 = new InventoryChange(from19, to19, InventoryType.Bag1, false);
                var from20 = Fixtures.GenerateItem(100, InventoryType.Bag1, 3, 16911, 5);
                var to20 = Fixtures.GenerateItem(100, InventoryType.Bag1, 3, 5118, 24);
                var change20 = new InventoryChange(from20, to20, InventoryType.Bag1, false);
                var from21 = Fixtures.GenerateItem(100, InventoryType.Bag1, 4, 19882, 4);
                var to21 = Fixtures.GenerateItem(100, InventoryType.Bag1, 4, 5213, 1);
                var change21 = new InventoryChange(from21, to21, InventoryType.Bag1, false);
                var from22 = Fixtures.GenerateItem(100, InventoryType.Bag1, 5, 27959, 14);
                var to22 = Fixtures.GenerateItem(100, InventoryType.Bag1, 5, 5069, 24);
                var change22 = new InventoryChange(from22, to22, InventoryType.Bag1, false);
                var from23 = Fixtures.GenerateItem(100, InventoryType.Bag1, 6, 36038, 1);
                var to23 = Fixtures.GenerateItem(100, InventoryType.Bag1, 6, 5068, 2);
                var change23 = new InventoryChange(from23, to23, InventoryType.Bag1, false);
                var from24 = Fixtures.GenerateItem(100, InventoryType.Bag1, 7, 30481, 1);
                var to24 = Fixtures.GenerateItem(100, InventoryType.Bag1, 7, 27710, 3);
                var change24 = new InventoryChange(from24, to24, InventoryType.Bag1, false);
                var from25 = Fixtures.GenerateItem(100, InventoryType.Bag1, 8, 27878, 1);
                var to25 = Fixtures.GenerateItem(100, InventoryType.Bag1, 8, 19934, 27);
                var change25 = new InventoryChange(from25, to25, InventoryType.Bag1, false);
                var from26 = Fixtures.GenerateItem(100, InventoryType.Bag1, 9, 27870, 5);
                var to26 = Fixtures.GenerateItem(100, InventoryType.Bag1, 9, 27684, 27);
                var change26 = new InventoryChange(from26, to26, InventoryType.Bag1, false);
                var from27 = Fixtures.GenerateItem(100, InventoryType.Bag1, 10, 27871, 2);
                var to27 = Fixtures.GenerateItem(100, InventoryType.Bag1, 10, 12598, 8);
                var change27 = new InventoryChange(from27, to27, InventoryType.Bag1, false);
                var from28 = Fixtures.GenerateItem(100, InventoryType.Bag1, 11, 4678, 1);
                var to28 = Fixtures.GenerateItem(100, InventoryType.Bag1, 11, 12598, 203);
                var change28 = new InventoryChange(from28, to28, InventoryType.Bag1, false);
                var from29 = Fixtures.GenerateItem(100, InventoryType.Bag1, 12, 4643, 3);
                var to29 = Fixtures.GenerateItem(100, InventoryType.Bag1, 12, 22493, 2);
                var change29 = new InventoryChange(from29, to29, InventoryType.Bag1, false);
                var from30 = Fixtures.GenerateItem(100, InventoryType.Bag1, 13, 4682, 1);
                var to30 = Fixtures.GenerateItem(100, InventoryType.Bag1, 13, 5287, 1);
                var change30 = new InventoryChange(from30, to30, InventoryType.Bag1, false);
                var from31 = Fixtures.GenerateItem(100, InventoryType.Bag1, 14, 4868, 192);
                var to31 = Fixtures.GenerateItem(100, InventoryType.Bag1, 14, 5320, 40);
                var change31 = new InventoryChange(from31, to31, InventoryType.Bag1, false);
                var from32 = Fixtures.GenerateItem(100, InventoryType.Bag1, 15, 16929, 16);
                var to32 = Fixtures.GenerateItem(100, InventoryType.Bag1, 15, 5435, 4);
                var change32 = new InventoryChange(from32, to32, InventoryType.Bag1, false);
                var from33 = Fixtures.GenerateItem(100, InventoryType.Bag1, 16, 0, 0);
                var to33 = Fixtures.GenerateItem(100, InventoryType.Bag1, 16, 19895, 1);
                var change33 = new InventoryChange(from33, to33, InventoryType.Bag1, false);
                var from34 = Fixtures.GenerateItem(100, InventoryType.Bag1, 17, 0, 0);
                var to34 = Fixtures.GenerateItem(100, InventoryType.Bag1, 17, 5473, 1);
                var change34 = new InventoryChange(from34, to34, InventoryType.Bag1, false);
                var from35 = Fixtures.GenerateItem(100, InventoryType.Bag1, 18, 0, 0);
                var to35 = Fixtures.GenerateItem(100, InventoryType.Bag1, 18, 5474, 1);
                var change35 = new InventoryChange(from35, to35, InventoryType.Bag1, false);
                var from36 = Fixtures.GenerateItem(100, InventoryType.Bag1, 19, 0, 0);
                var to36 = Fixtures.GenerateItem(100, InventoryType.Bag1, 19, 27765, 2);
                var change36 = new InventoryChange(from36, to36, InventoryType.Bag1, false);
                var from37 = Fixtures.GenerateItem(100, InventoryType.Bag1, 20, 0, 0);
                var to37 = Fixtures.GenerateItem(100, InventoryType.Bag1, 20, 5476, 2);
                var change37 = new InventoryChange(from37, to37, InventoryType.Bag1, false);
                var from38 = Fixtures.GenerateItem(100, InventoryType.Bag1, 21, 0, 0);
                var to38 = Fixtures.GenerateItem(100, InventoryType.Bag1, 21, 27781, 2);
                var change38 = new InventoryChange(from38, to38, InventoryType.Bag1, false);
                var from39 = Fixtures.GenerateItem(100, InventoryType.Bag1, 22, 0, 0);
                var to39 = Fixtures.GenerateItem(100, InventoryType.Bag1, 22, 5491, 22);
                var change39 = new InventoryChange(from39, to39, InventoryType.Bag1, false);
                var from40 = Fixtures.GenerateItem(100, InventoryType.Bag1, 23, 0, 0);
                var to40 = Fixtures.GenerateItem(100, InventoryType.Bag1, 23, 5509, 99);
                var change40 = new InventoryChange(from40, to40, InventoryType.Bag1, false);
                var from41 = Fixtures.GenerateItem(100, InventoryType.Bag1, 24, 0, 0);
                var to41 = Fixtures.GenerateItem(100, InventoryType.Bag1, 24, 5510, 87);
                var change41 = new InventoryChange(from41, to41, InventoryType.Bag1, false);
                var from42 = Fixtures.GenerateItem(100, InventoryType.Bag1, 25, 0, 0);
                var to42 = Fixtures.GenerateItem(100, InventoryType.Bag1, 25, 5515, 4);
                var change42 = new InventoryChange(from42, to42, InventoryType.Bag1, false);
                var from43 = Fixtures.GenerateItem(100, InventoryType.Bag1, 26, 0, 0);
                var to43 = Fixtures.GenerateItem(100, InventoryType.Bag1, 26, 5523, 53);
                var change43 = new InventoryChange(from43, to43, InventoryType.Bag1, false);
                var from44 = Fixtures.GenerateItem(100, InventoryType.Bag1, 27, 0, 0);
                var to44 = Fixtures.GenerateItem(100, InventoryType.Bag1, 27, 5528, 14);
                var change44 = new InventoryChange(from44, to44, InventoryType.Bag1, false);
                var from45 = Fixtures.GenerateItem(100, InventoryType.Bag1, 28, 0, 0);
                var to45 = Fixtures.GenerateItem(100, InventoryType.Bag1, 28, 5512, 32);
                var change45 = new InventoryChange(from45, to45, InventoryType.Bag1, false);
                var from46 = Fixtures.GenerateItem(100, InventoryType.Bag1, 29, 0, 0);
                var to46 = Fixtures.GenerateItem(100, InventoryType.Bag1, 29, 5538, 2);
                var change46 = new InventoryChange(from46, to46, InventoryType.Bag1, false);
                var from47 = Fixtures.GenerateItem(100, InventoryType.Bag1, 30, 0, 0);
                var to47 = Fixtures.GenerateItem(100, InventoryType.Bag1, 30, 7031, 2);
                var change47 = new InventoryChange(from47, to47, InventoryType.Bag1, false);
                var from48 = Fixtures.GenerateItem(100, InventoryType.Bag1, 31, 0, 0);
                var to48 = Fixtures.GenerateItem(100, InventoryType.Bag1, 31, 13757, 1);
                var change48 = new InventoryChange(from48, to48, InventoryType.Bag1, false);
                var from49 = Fixtures.GenerateItem(100, InventoryType.Bag1, 32, 0, 0);
                var to49 = Fixtures.GenerateItem(100, InventoryType.Bag1, 32, 5559, 2);
                var change49 = new InventoryChange(from49, to49, InventoryType.Bag1, false);
                var from50 = Fixtures.GenerateItem(100, InventoryType.Bag1, 33, 0, 0);
                var to50 = Fixtures.GenerateItem(100, InventoryType.Bag1, 33, 5563, 2);
                var change50 = new InventoryChange(from50, to50, InventoryType.Bag1, false);
                var from51 = Fixtures.GenerateItem(100, InventoryType.Bag1, 34, 0, 0);
                var to51 = Fixtures.GenerateItem(100, InventoryType.Bag1, 34, 22425, 2);
                var change51 = new InventoryChange(from51, to51, InventoryType.Bag1, false);
                var from52 = Fixtures.GenerateItem(100, InventoryType.Bag2, 0, 13717, 2);
                var to52 = Fixtures.GenerateItem(100, InventoryType.Bag2, 0, 27795, 1);
                var change52 = new InventoryChange(from52, to52, InventoryType.Bag2, false);
                var from53 = Fixtures.GenerateItem(100, InventoryType.Bag2, 1, 5118, 24);
                var to53 = Fixtures.GenerateItem(100, InventoryType.Bag2, 1, 24259, 1);
                var change53 = new InventoryChange(from53, to53, InventoryType.Bag2, false);
                var from54 = Fixtures.GenerateItem(100, InventoryType.Bag2, 2, 27701, 30);
                var to54 = Fixtures.GenerateItem(100, InventoryType.Bag2, 2, 36240, 1);
                var change54 = new InventoryChange(from54, to54, InventoryType.Bag2, false);
                var from55 = Fixtures.GenerateItem(100, InventoryType.Bag2, 3, 27698, 28);
                var to55 = Fixtures.GenerateItem(100, InventoryType.Bag2, 3, 7770, 2);
                var change55 = new InventoryChange(from55, to55, InventoryType.Bag2, false);
                var from56 = Fixtures.GenerateItem(100, InventoryType.Bag2, 4, 5213, 1);
                var to56 = Fixtures.GenerateItem(100, InventoryType.Bag2, 4, 8150, 2);
                var change56 = new InventoryChange(from56, to56, InventoryType.Bag2, false);
                var from57 = Fixtures.GenerateItem(100, InventoryType.Bag2, 5, 5259, 2);
                var to57 = Fixtures.GenerateItem(100, InventoryType.Bag2, 5, 12254, 2);
                var change57 = new InventoryChange(from57, to57, InventoryType.Bag2, false);
                var from58 = Fixtures.GenerateItem(100, InventoryType.Bag2, 6, 5068, 2);
                var to58 = Fixtures.GenerateItem(100, InventoryType.Bag2, 6, 24566, 1);
                var change58 = new InventoryChange(from58, to58, InventoryType.Bag2, false);
                var from59 = Fixtures.GenerateItem(100, InventoryType.Bag2, 7, 5069, 24);
                var to59 = Fixtures.GenerateItem(100, InventoryType.Bag2, 7, 24566, 1);
                var change59 = new InventoryChange(from59, to59, InventoryType.Bag2, false);
                var from60 = Fixtures.GenerateItem(100, InventoryType.Bag2, 8, 27710, 3);
                var to60 = Fixtures.GenerateItem(100, InventoryType.Bag2, 8, 24566, 1);
                var change60 = new InventoryChange(from60, to60, InventoryType.Bag2, false);
                var from61 = Fixtures.GenerateItem(100, InventoryType.Bag2, 9, 19934, 27);
                var to61 = Fixtures.GenerateItem(100, InventoryType.Bag2, 9, 31975, 3);
                var change61 = new InventoryChange(from61, to61, InventoryType.Bag2, false);
                var from62 = Fixtures.GenerateItem(100, InventoryType.Bag2, 10, 27684, 27);
                var to62 = Fixtures.GenerateItem(100, InventoryType.Bag2, 10, 31973, 9);
                var change62 = new InventoryChange(from62, to62, InventoryType.Bag2, false);
                var from63 = Fixtures.GenerateItem(100, InventoryType.Bag2, 11, 22493, 2);
                var to63 = Fixtures.GenerateItem(100, InventoryType.Bag2, 11, 31978, 13);
                var change63 = new InventoryChange(from63, to63, InventoryType.Bag2, false);
                var from64 = Fixtures.GenerateItem(100, InventoryType.Bag2, 12, 12598, 8);
                var to64 = Fixtures.GenerateItem(100, InventoryType.Bag2, 12, 38621, 1);
                var change64 = new InventoryChange(from64, to64, InventoryType.Bag2, false);
                var from65 = Fixtures.GenerateItem(100, InventoryType.Bag2, 13, 12598, 203);
                var to65 = Fixtures.GenerateItem(100, InventoryType.Bag2, 13, 37357, 1);
                var change65 = new InventoryChange(from65, to65, InventoryType.Bag2, false);
                var from66 = Fixtures.GenerateItem(100, InventoryType.Bag2, 14, 5287, 1);
                var to66 = Fixtures.GenerateItem(100, InventoryType.Bag2, 14, 33281, 1);
                var change66 = new InventoryChange(from66, to66, InventoryType.Bag2, false);
                var from67 = Fixtures.GenerateItem(100, InventoryType.Bag2, 15, 5320, 40);
                var to67 = Fixtures.GenerateItem(100, InventoryType.Bag2, 15, 12089, 1);
                var change67 = new InventoryChange(from67, to67, InventoryType.Bag2, false);
                var from68 = Fixtures.GenerateItem(100, InventoryType.Bag2, 16, 5435, 4);
                var to68 = Fixtures.GenerateItem(100, InventoryType.Bag2, 16, 33272, 1);
                var change68 = new InventoryChange(from68, to68, InventoryType.Bag2, false);
                var from69 = Fixtures.GenerateItem(100, InventoryType.Bag2, 17, 5473, 1);
                var to69 = Fixtures.GenerateItem(100, InventoryType.Bag2, 17, 24492, 1);
                var change69 = new InventoryChange(from69, to69, InventoryType.Bag2, false);
                var from70 = Fixtures.GenerateItem(100, InventoryType.Bag2, 18, 5474, 1);
                var to70 = Fixtures.GenerateItem(100, InventoryType.Bag2, 18, 7722, 1);
                var change70 = new InventoryChange(from70, to70, InventoryType.Bag2, false);
                var from71 = Fixtures.GenerateItem(100, InventoryType.Bag2, 19, 27765, 2);
                var to71 = Fixtures.GenerateItem(100, InventoryType.Bag2, 19, 15855, 3);
                var change71 = new InventoryChange(from71, to71, InventoryType.Bag2, false);
                var from72 = Fixtures.GenerateItem(100, InventoryType.Bag2, 20, 5476, 2);
                var to72 = Fixtures.GenerateItem(100, InventoryType.Bag2, 20, 4868, 192);
                var change72 = new InventoryChange(from72, to72, InventoryType.Bag2, false);
                var from73 = Fixtures.GenerateItem(100, InventoryType.Bag2, 21, 27781, 2);
                var to73 = Fixtures.GenerateItem(100, InventoryType.Bag2, 21, 0, 0);
                var change73 = new InventoryChange(from73, to73, InventoryType.Bag2, false);
                var from74 = Fixtures.GenerateItem(100, InventoryType.Bag2, 22, 5491, 22);
                var to74 = Fixtures.GenerateItem(100, InventoryType.Bag2, 22, 0, 0);
                var change74 = new InventoryChange(from74, to74, InventoryType.Bag2, false);
                var from75 = Fixtures.GenerateItem(100, InventoryType.Bag2, 23, 5509, 99);
                var to75 = Fixtures.GenerateItem(100, InventoryType.Bag2, 23, 14935, 2);
                var change75 = new InventoryChange(from75, to75, InventoryType.Bag2, false);
                var from76 = Fixtures.GenerateItem(100, InventoryType.Bag2, 24, 5510, 87);
                var to76 = Fixtures.GenerateItem(100, InventoryType.Bag2, 24, 10335, 70);
                var change76 = new InventoryChange(from76, to76, InventoryType.Bag2, false);
                var from77 = Fixtures.GenerateItem(100, InventoryType.Bag2, 25, 5515, 4);
                var to77 = Fixtures.GenerateItem(100, InventoryType.Bag2, 25, 10155, 457);
                var change77 = new InventoryChange(from77, to77, InventoryType.Bag2, false);
                var from78 = Fixtures.GenerateItem(100, InventoryType.Bag2, 26, 5523, 53);
                var to78 = Fixtures.GenerateItem(100, InventoryType.Bag2, 26, 10373, 82);
                var change78 = new InventoryChange(from78, to78, InventoryType.Bag2, false);
                var from79 = Fixtures.GenerateItem(100, InventoryType.Bag2, 27, 5528, 14);
                var to79 = Fixtures.GenerateItem(100, InventoryType.Bag2, 27, 22317, 3);
                var change79 = new InventoryChange(from79, to79, InventoryType.Bag2, false);
                var from80 = Fixtures.GenerateItem(100, InventoryType.Bag2, 28, 36240, 1);
                var to80 = Fixtures.GenerateItem(100, InventoryType.Bag2, 28, 8143, 1);
                var change80 = new InventoryChange(from80, to80, InventoryType.Bag2, false);
                var from81 = Fixtures.GenerateItem(100, InventoryType.Bag2, 29, 5512, 32);
                var to81 = Fixtures.GenerateItem(100, InventoryType.Bag2, 29, 13627, 3);
                var change81 = new InventoryChange(from81, to81, InventoryType.Bag2, false);
                var from82 = Fixtures.GenerateItem(100, InventoryType.Bag2, 30, 5538, 2);
                var to82 = Fixtures.GenerateItem(100, InventoryType.Bag2, 30, 22444, 1);
                var change82 = new InventoryChange(from82, to82, InventoryType.Bag2, false);
                var from83 = Fixtures.GenerateItem(100, InventoryType.Bag2, 31, 7031, 2);
                var to83 = Fixtures.GenerateItem(100, InventoryType.Bag2, 31, 33870, 11);
                var change83 = new InventoryChange(from83, to83, InventoryType.Bag2, false);
                var from84 = Fixtures.GenerateItem(100, InventoryType.Bag2, 32, 7770, 2);
                var to84 = Fixtures.GenerateItem(100, InventoryType.Bag2, 32, 39365, 3);
                var change84 = new InventoryChange(from84, to84, InventoryType.Bag2, false);
                var from85 = Fixtures.GenerateItem(100, InventoryType.Bag2, 33, 13757, 1);
                var to85 = Fixtures.GenerateItem(100, InventoryType.Bag2, 33, 14283, 1);
                var change85 = new InventoryChange(from85, to85, InventoryType.Bag2, false);
                var from86 = Fixtures.GenerateItem(100, InventoryType.Bag2, 34, 5559, 2);
                var to86 = Fixtures.GenerateItem(100, InventoryType.Bag2, 34, 16929, 16);
                var change86 = new InventoryChange(from86, to86, InventoryType.Bag2, false);
                var from87 = Fixtures.GenerateItem(100, InventoryType.Bag3, 0, 5563, 2);
                var to87 = Fixtures.GenerateItem(100, InventoryType.Bag3, 0, 13357, 4);
                var change87 = new InventoryChange(from87, to87, InventoryType.Bag3, false);
                var from88 = Fixtures.GenerateItem(100, InventoryType.Bag3, 1, 19895, 1);
                var to88 = Fixtures.GenerateItem(100, InventoryType.Bag3, 1, 12230, 15);
                var change88 = new InventoryChange(from88, to88, InventoryType.Bag3, false);
                var from89 = Fixtures.GenerateItem(100, InventoryType.Bag3, 2, 22425, 2);
                var to89 = Fixtures.GenerateItem(100, InventoryType.Bag3, 2, 0, 0);
                var change89 = new InventoryChange(from89, to89, InventoryType.Bag3, false);
                var from90 = Fixtures.GenerateItem(100, InventoryType.Bag3, 3, 24259, 1);
                var to90 = Fixtures.GenerateItem(100, InventoryType.Bag3, 3, 0, 0);
                var change90 = new InventoryChange(from90, to90, InventoryType.Bag3, false);
                var from91 = Fixtures.GenerateItem(100, InventoryType.Bag3, 4, 27795, 1);
                var to91 = Fixtures.GenerateItem(100, InventoryType.Bag3, 4, 0, 0);
                var change91 = new InventoryChange(from91, to91, InventoryType.Bag3, false);
                var from92 = Fixtures.GenerateItem(100, InventoryType.Bag3, 5, 8150, 2);
                var to92 = Fixtures.GenerateItem(100, InventoryType.Bag3, 5, 0, 0);
                var change92 = new InventoryChange(from92, to92, InventoryType.Bag3, false);
                var from93 = Fixtures.GenerateItem(100, InventoryType.Bag3, 6, 12254, 2);
                var to93 = Fixtures.GenerateItem(100, InventoryType.Bag3, 6, 0, 0);
                var change93 = new InventoryChange(from93, to93, InventoryType.Bag3, false);
                var from94 = Fixtures.GenerateItem(100, InventoryType.Bag3, 7, 24566, 1);
                var to94 = Fixtures.GenerateItem(100, InventoryType.Bag3, 7, 0, 0);
                var change94 = new InventoryChange(from94, to94, InventoryType.Bag3, false);
                var from95 = Fixtures.GenerateItem(100, InventoryType.Bag3, 8, 24566, 1);
                var to95 = Fixtures.GenerateItem(100, InventoryType.Bag3, 8, 0, 0);
                var change95 = new InventoryChange(from95, to95, InventoryType.Bag3, false);
                var from96 = Fixtures.GenerateItem(100, InventoryType.Bag3, 9, 24566, 1);
                var to96 = Fixtures.GenerateItem(100, InventoryType.Bag3, 9, 0, 0);
                var change96 = new InventoryChange(from96, to96, InventoryType.Bag3, false);
                var from97 = Fixtures.GenerateItem(100, InventoryType.Bag3, 10, 31975, 3);
                var to97 = Fixtures.GenerateItem(100, InventoryType.Bag3, 10, 0, 0);
                var change97 = new InventoryChange(from97, to97, InventoryType.Bag3, false);
                var from98 = Fixtures.GenerateItem(100, InventoryType.Bag3, 11, 31973, 9);
                var to98 = Fixtures.GenerateItem(100, InventoryType.Bag3, 11, 0, 0);
                var change98 = new InventoryChange(from98, to98, InventoryType.Bag3, false);
                var from99 = Fixtures.GenerateItem(100, InventoryType.Bag3, 12, 31978, 13);
                var to99 = Fixtures.GenerateItem(100, InventoryType.Bag3, 12, 0, 0);
                var change99 = new InventoryChange(from99, to99, InventoryType.Bag3, false);
                var from100 = Fixtures.GenerateItem(100, InventoryType.Bag3, 13, 38621, 1);
                var to100 = Fixtures.GenerateItem(100, InventoryType.Bag3, 13, 0, 0);
                var change100 = new InventoryChange(from100, to100, InventoryType.Bag3, false);
                var from101 = Fixtures.GenerateItem(100, InventoryType.Bag3, 14, 37357, 1);
                var to101 = Fixtures.GenerateItem(100, InventoryType.Bag3, 14, 0, 0);
                var change101 = new InventoryChange(from101, to101, InventoryType.Bag3, false);
                var from102 = Fixtures.GenerateItem(100, InventoryType.Bag3, 15, 33281, 1);
                var to102 = Fixtures.GenerateItem(100, InventoryType.Bag3, 15, 0, 0);
                var change102 = new InventoryChange(from102, to102, InventoryType.Bag3, false);
                var from103 = Fixtures.GenerateItem(100, InventoryType.Bag3, 16, 12089, 1);
                var to103 = Fixtures.GenerateItem(100, InventoryType.Bag3, 16, 0, 0);
                var change103 = new InventoryChange(from103, to103, InventoryType.Bag3, false);
                var from104 = Fixtures.GenerateItem(100, InventoryType.Bag3, 17, 33272, 1);
                var to104 = Fixtures.GenerateItem(100, InventoryType.Bag3, 17, 0, 0);
                var change104 = new InventoryChange(from104, to104, InventoryType.Bag3, false);
                var from105 = Fixtures.GenerateItem(100, InventoryType.Bag3, 18, 24492, 1);
                var to105 = Fixtures.GenerateItem(100, InventoryType.Bag3, 18, 0, 0);
                var change105 = new InventoryChange(from105, to105, InventoryType.Bag3, false);
                var from106 = Fixtures.GenerateItem(100, InventoryType.Bag3, 19, 7722, 1);
                var to106 = Fixtures.GenerateItem(100, InventoryType.Bag3, 19, 0, 0);
                var change106 = new InventoryChange(from106, to106, InventoryType.Bag3, false);
                var from107 = Fixtures.GenerateItem(100, InventoryType.Bag3, 20, 15855, 3);
                var to107 = Fixtures.GenerateItem(100, InventoryType.Bag3, 20, 0, 0);
                var change107 = new InventoryChange(from107, to107, InventoryType.Bag3, false);
                var from108 = Fixtures.GenerateItem(100, InventoryType.Bag3, 21, 12230, 15);
                var to108 = Fixtures.GenerateItem(100, InventoryType.Bag3, 21, 0, 0);
                var change108 = new InventoryChange(from108, to108, InventoryType.Bag3, false);
                var from109 = Fixtures.GenerateItem(100, InventoryType.Bag3, 22, 14935, 2);
                var to109 = Fixtures.GenerateItem(100, InventoryType.Bag3, 22, 0, 0);
                var change109 = new InventoryChange(from109, to109, InventoryType.Bag3, false);
                var from110 = Fixtures.GenerateItem(100, InventoryType.Bag3, 23, 10335, 70);
                var to110 = Fixtures.GenerateItem(100, InventoryType.Bag3, 23, 0, 0);
                var change110 = new InventoryChange(from110, to110, InventoryType.Bag3, false);
                var from111 = Fixtures.GenerateItem(100, InventoryType.Bag3, 24, 10155, 457);
                var to111 = Fixtures.GenerateItem(100, InventoryType.Bag3, 24, 0, 0);
                var change111 = new InventoryChange(from111, to111, InventoryType.Bag3, false);
                var from112 = Fixtures.GenerateItem(100, InventoryType.Bag3, 25, 10373, 82);
                var to112 = Fixtures.GenerateItem(100, InventoryType.Bag3, 25, 0, 0);
                var change112 = new InventoryChange(from112, to112, InventoryType.Bag3, false);
                var from113 = Fixtures.GenerateItem(100, InventoryType.Bag3, 26, 22317, 3);
                var to113 = Fixtures.GenerateItem(100, InventoryType.Bag3, 26, 0, 0);
                var change113 = new InventoryChange(from113, to113, InventoryType.Bag3, false);
                var from114 = Fixtures.GenerateItem(100, InventoryType.Bag3, 27, 8143, 1);
                var to114 = Fixtures.GenerateItem(100, InventoryType.Bag3, 27, 0, 0);
                var change114 = new InventoryChange(from114, to114, InventoryType.Bag3, false);
                var from115 = Fixtures.GenerateItem(100, InventoryType.Bag3, 28, 13627, 3);
                var to115 = Fixtures.GenerateItem(100, InventoryType.Bag3, 28, 0, 0);
                var change115 = new InventoryChange(from115, to115, InventoryType.Bag3, false);
                var from116 = Fixtures.GenerateItem(100, InventoryType.Bag3, 29, 22444, 1);
                var to116 = Fixtures.GenerateItem(100, InventoryType.Bag3, 29, 0, 0);
                var change116 = new InventoryChange(from116, to116, InventoryType.Bag3, false);
                var from117 = Fixtures.GenerateItem(100, InventoryType.Bag3, 30, 33870, 11);
                var to117 = Fixtures.GenerateItem(100, InventoryType.Bag3, 30, 0, 0);
                var change117 = new InventoryChange(from117, to117, InventoryType.Bag3, false);
                var from118 = Fixtures.GenerateItem(100, InventoryType.Bag3, 31, 39365, 3);
                var to118 = Fixtures.GenerateItem(100, InventoryType.Bag3, 31, 0, 0);
                var change118 = new InventoryChange(from118, to118, InventoryType.Bag3, false);
                var from119 = Fixtures.GenerateItem(100, InventoryType.Bag3, 32, 14283, 1);
                var to119 = Fixtures.GenerateItem(100, InventoryType.Bag3, 32, 0, 0);
                var change119 = new InventoryChange(from119, to119, InventoryType.Bag3, false);
                var from120 = Fixtures.GenerateItem(100, InventoryType.Bag3, 33, 13357, 4);
                var to120 = Fixtures.GenerateItem(100, InventoryType.Bag3, 33, 0, 0);
                var change120 = new InventoryChange(from120, to120, InventoryType.Bag3, false);       
                
                var changes = new List<InventoryChange>();;
                changes.Add(change0); 
                changes.Add(change1); 
                changes.Add(change2); 
                changes.Add(change3); 
                changes.Add(change4); 
                changes.Add(change5); 
                changes.Add(change6); 
                changes.Add(change7); 
                changes.Add(change8); 
                changes.Add(change9); 
                changes.Add(change10); 
                changes.Add(change11); 
                changes.Add(change12); 
                changes.Add(change13); 
                changes.Add(change14); 
                changes.Add(change15); 
                changes.Add(change16); 
                changes.Add(change17); 
                changes.Add(change18); 
                changes.Add(change19); 
                changes.Add(change20); 
                changes.Add(change21); 
                changes.Add(change22); 
                changes.Add(change23); 
                changes.Add(change24); 
                changes.Add(change25); 
                changes.Add(change26); 
                changes.Add(change27); 
                changes.Add(change28); 
                changes.Add(change29); 
                changes.Add(change30); 
                changes.Add(change31); 
                changes.Add(change32); 
                changes.Add(change33); 
                changes.Add(change34); 
                changes.Add(change35); 
                changes.Add(change36); 
                changes.Add(change37); 
                changes.Add(change38); 
                changes.Add(change39); 
                changes.Add(change40); 
                changes.Add(change41); 
                changes.Add(change42); 
                changes.Add(change43); 
                changes.Add(change44); 
                changes.Add(change45); 
                changes.Add(change46); 
                changes.Add(change47); 
                changes.Add(change48); 
                changes.Add(change49); 
                changes.Add(change50); 
                changes.Add(change51); 
                changes.Add(change52); 
                changes.Add(change53); 
                changes.Add(change54); 
                changes.Add(change55); 
                changes.Add(change56); 
                changes.Add(change57); 
                changes.Add(change58); 
                changes.Add(change59); 
                changes.Add(change60); 
                changes.Add(change61); 
                changes.Add(change62); 
                changes.Add(change63); 
                changes.Add(change64); 
                changes.Add(change65); 
                changes.Add(change66); 
                changes.Add(change67); 
                changes.Add(change68); 
                changes.Add(change69); 
                changes.Add(change70); 
                changes.Add(change71); 
                changes.Add(change72); 
                changes.Add(change73); 
                changes.Add(change74); 
                changes.Add(change75); 
                changes.Add(change76); 
                changes.Add(change77); 
                changes.Add(change78); 
                changes.Add(change79); 
                changes.Add(change80); 
                changes.Add(change81); 
                changes.Add(change82); 
                changes.Add(change83); 
                changes.Add(change84); 
                changes.Add(change85); 
                changes.Add(change86); 
                changes.Add(change87); 
                changes.Add(change88); 
                changes.Add(change89); 
                changes.Add(change90); 
                changes.Add(change91); 
                changes.Add(change92); 
                changes.Add(change93); 
                changes.Add(change94); 
                changes.Add(change95); 
                changes.Add(change96); 
                changes.Add(change97); 
                changes.Add(change98); 
                changes.Add(change99); 
                changes.Add(change100); 
                changes.Add(change101); 
                changes.Add(change102); 
                changes.Add(change103); 

                
                var processedChanges = _inventoryHistory.AnalyzeInventoryChanges(changes);

                Assert.AreEqual(true, processedChanges.All(c => c.InventoryChangeReason == InventoryChangeReason.Moved));
            }
        }
    }
}