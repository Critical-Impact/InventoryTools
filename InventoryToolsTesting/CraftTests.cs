using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using NUnit.Framework;
using Lumina;

namespace InventoryToolsTesting
{
    [TestFixture]
    public class CraftTests
    {

        [OneTimeSetUp]
        public void Init()
        {
            var lumina = new GameData( "C:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack", new LuminaOptions()
            {
                PanicOnSheetChecksumMismatch = false
            } );
            Service.ExcelCache = new ExcelCache(lumina);
        }

        [Test]
        public void TestSkybuildersCalculations()
        {
            CraftList list = new CraftList();
            list.AddCraftItem(31922, 2);
            var requiredMaterialsList = list.GetRequiredMaterialsList();
            Assert.AreEqual( 10, requiredMaterialsList[32014]);
            
            list = new CraftList();
            //Skybuilders bed
            list.AddCraftItem(31945, 1);
            requiredMaterialsList = list.GetRequiredMaterialsList();
            Assert.AreEqual( 10, requiredMaterialsList[32028]);
        }

        [Test]
        public void TestYields()
        {
            {
                CraftList list = new CraftList();
                list.AddCraftItem("Cotton Yarn", 3);
                var requiredMaterialsList = list.GetRequiredMaterialsListNamed();
                Assert.AreEqual(4, requiredMaterialsList["Cotton Boll"]);

                var cottonBolls = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Cotton Boll");
                Assert.AreEqual(4, cottonBolls.QuantityRequired);
                Assert.AreEqual(4, cottonBolls.QuantityNeeded);
                Assert.AreEqual(0, cottonBolls.QuantityReady);
                Assert.AreEqual(0, cottonBolls.QuantityAvailable);
                Assert.AreEqual(0, cottonBolls.QuantityWillRetrieve);
                Assert.AreEqual(4, cottonBolls.QuantityMissingOverall);
                Assert.AreEqual(4, cottonBolls.QuantityMissingInventory);
                Assert.AreEqual(0, cottonBolls.CraftOperationsRequired);
            }


            {
                CraftList list = new CraftList();
                list.AddCraftItem("Cotton Yarn", 3);
                var requiredMaterialsList = list.GetRequiredMaterialsListNamed();
                Assert.AreEqual(4, requiredMaterialsList["Cotton Boll"]);
                var sourceStore = new CraftItemSourceStore()
                    .AddCharacterSource("Cotton Boll", 4, false)
                    .AddCharacterSource("Lightning Shard", 4, false);
                list.Update(sourceStore);
                
                var cottonBolls = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Cotton Boll");
                Assert.AreEqual(4, cottonBolls.QuantityRequired);
                Assert.AreEqual(0, cottonBolls.QuantityNeeded);
                Assert.AreEqual(4, cottonBolls.QuantityReady);
                Assert.AreEqual(0, cottonBolls.QuantityAvailable);
                Assert.AreEqual(0, cottonBolls.QuantityWillRetrieve);
                Assert.AreEqual(0, cottonBolls.QuantityMissingOverall);
                Assert.AreEqual(0, cottonBolls.QuantityMissingInventory);
                Assert.AreEqual(0, cottonBolls.CraftOperationsRequired);
                
                var cottonYarn = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Cotton Yarn");
                Assert.AreEqual(3, cottonYarn.QuantityRequired);
                Assert.AreEqual(3, cottonYarn.QuantityNeeded);
                Assert.AreEqual(0, cottonYarn.QuantityReady);
                Assert.AreEqual(0, cottonYarn.QuantityAvailable);
                Assert.AreEqual(0, cottonYarn.QuantityWillRetrieve);
                Assert.AreEqual(3, cottonYarn.QuantityMissingOverall);
                Assert.AreEqual(3, cottonYarn.QuantityMissingInventory);
                Assert.AreEqual(2, cottonYarn.CraftOperationsRequired);
                Assert.AreEqual(3, cottonYarn.QuantityCanCraft);
            }
        }

        [Test]
        public void TestLeftovers()
        {
            //Testing how partial used ingredients of a craft can then be used in crafts in the same list
            CraftList list = new CraftList();
            list.AddCraftItem(38930, 300);
            list.AddCraftItem(38929, 300);
            var requiredMaterialsList = list.GetQuantityNeededList();
            Assert.AreEqual( 600, requiredMaterialsList[36085]);
        }
        
        [Test]
        public void TestRecipeCalculation()
        {
            CraftList list = new CraftList();
            //Rivera Bed
            list.AddCraftItem(6542, 2);

            var requiredMaterialsList = list.GetRequiredMaterialsList();
            //undyed cotton cloth
            Assert.AreEqual( 4, requiredMaterialsList[5325]);
            //cotton yarn
            Assert.AreEqual(8, requiredMaterialsList[5334]);
            //cotton boll
            Assert.AreEqual(8, requiredMaterialsList[5343]);
            //maple lumber
            Assert.AreEqual(4, requiredMaterialsList[5361]);
            //maple log
            Assert.AreEqual(12, requiredMaterialsList[5380]);
            //bronze ingot
            Assert.AreEqual(4, requiredMaterialsList[5056]);
            //copper ore
            Assert.AreEqual(8, requiredMaterialsList[5106]);
            //tin ore
            Assert.AreEqual(4, requiredMaterialsList[5107]);
            //fire shard
            Assert.AreEqual(4, requiredMaterialsList[2]);
            //lightning shard
            Assert.AreEqual(10, requiredMaterialsList[6]);
            //wind shard
            Assert.AreEqual(6, requiredMaterialsList[4]);

            list = new CraftList();
            list.AddCraftItem(27877, 1);
            requiredMaterialsList = list.GetRequiredMaterialsList();
            Assert.AreEqual( 2, requiredMaterialsList[27830]);
        }
        
        [Test]
        public void TestCraftableQuantites()
        {
            CraftList list = new CraftList();
            //Rivera Bed
            list.AddCraftItem(11990, 6);
            list.AddCraftItem(11957, 6);
            list.GenerateCraftChildren();
            
            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var stalkRamie = new CraftItemSource(12598, 319, false);
            characterMaterials.Add(stalkRamie.ItemId, new List<CraftItemSource>() {stalkRamie});
            var flax = new CraftItemSource(5346, 275, false);
            characterMaterials.Add(flax.ItemId, new List<CraftItemSource>() {flax});
            var lightningCrystal = new CraftItemSource(12, 999, false);
            characterMaterials.Add(lightningCrystal.ItemId, new List<CraftItemSource>() {lightningCrystal});
            var earthCrystal = new CraftItemSource(11, 999, false);
            characterMaterials.Add(earthCrystal.ItemId, new List<CraftItemSource>() {earthCrystal});
            var windCrystal = new CraftItemSource(10, 999, false);
            characterMaterials.Add(windCrystal.ItemId, new List<CraftItemSource>() {windCrystal});
            
            list.Update(characterMaterials, externalSources);
            //TODO: Finish this
        }

        [Test]
        public void TestQuantityAvailableCalculation()
        {
            CraftList list;
            CraftItemSourceStore sourceStore;
            Dictionary<string, uint> availableMaterialsList;
            Dictionary<string, uint> missingMaterialsList;
            Dictionary<string, uint> readyMaterialsList;
            Dictionary<string, uint> quantityNeededList;
            
            //Check the amount of available materials is 4 when provided with 4 externally
            list = new CraftList()
                .AddCraftItem("Riviera Bed", 2);
            sourceStore = new CraftItemSourceStore()
                .AddExternalSource("Undyed Cotton Cloth", 4, false);
            list.Update(sourceStore);
            availableMaterialsList = list.GetAvailableMaterialsListNamed();
            Assert.AreEqual(4, availableMaterialsList["Undyed Cotton Cloth"]);
            missingMaterialsList = list.GetMissingMaterialsListNamed();
            Assert.AreEqual(0, missingMaterialsList["Cotton Boll"]);
            Assert.AreEqual(0, missingMaterialsList["Cotton Yarn"]);

            var cottonBolls = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Cotton Boll");
            Assert.AreEqual(8, cottonBolls.QuantityRequired);
            Assert.AreEqual(0, cottonBolls.QuantityNeeded);
            Assert.AreEqual(0, cottonBolls.QuantityAvailable);
            Assert.AreEqual(0, cottonBolls.QuantityWillRetrieve);
            Assert.AreEqual(0, cottonBolls.QuantityMissingOverall);
            Assert.AreEqual(0, cottonBolls.QuantityMissingInventory);

            var undyedCottonCloth = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Undyed Cotton Cloth");
            Assert.AreEqual(4, undyedCottonCloth.QuantityRequired);
            Assert.AreEqual(0, undyedCottonCloth.QuantityNeeded);
            Assert.AreEqual(0, undyedCottonCloth.QuantityReady);
            Assert.AreEqual(4, undyedCottonCloth.QuantityAvailable);
            Assert.AreEqual(4, undyedCottonCloth.QuantityWillRetrieve);
            Assert.AreEqual(0, undyedCottonCloth.QuantityMissingOverall);
            Assert.AreEqual(4, undyedCottonCloth.QuantityMissingInventory);
                
            //Check the amount of missing materials is 4 for each item when provided with 2 externally
            list = new CraftList()
                .AddCraftItem("Riviera Bed", 2);
            sourceStore = new CraftItemSourceStore()
                .AddExternalSource("Undyed Cotton Cloth", 2, false);
            list.Update(sourceStore);
            availableMaterialsList = list.GetAvailableMaterialsListNamed();
            Assert.AreEqual(2, availableMaterialsList["Undyed Cotton Cloth"]);
            missingMaterialsList = list.GetMissingMaterialsListNamed();
            Assert.AreEqual(4, missingMaterialsList["Cotton Boll"]);
            Assert.AreEqual(4, missingMaterialsList["Cotton Yarn"]);
            
            cottonBolls = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Cotton Boll");
            Assert.AreEqual(8, cottonBolls.QuantityRequired);
            Assert.AreEqual(4, cottonBolls.QuantityNeeded);
            Assert.AreEqual(0, cottonBolls.QuantityAvailable);
            Assert.AreEqual(0, cottonBolls.QuantityWillRetrieve);
            Assert.AreEqual(4, cottonBolls.QuantityMissingOverall);
            Assert.AreEqual(4, cottonBolls.QuantityMissingInventory);

            undyedCottonCloth = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Undyed Cotton Cloth");
            Assert.AreEqual(4, undyedCottonCloth.QuantityRequired);
            Assert.AreEqual(2, undyedCottonCloth.QuantityNeeded);
            Assert.AreEqual(0, undyedCottonCloth.QuantityReady);
            Assert.AreEqual(2, undyedCottonCloth.QuantityAvailable);
            Assert.AreEqual(2, undyedCottonCloth.QuantityWillRetrieve);
            Assert.AreEqual(2, undyedCottonCloth.QuantityMissingOverall);
            Assert.AreEqual(4, undyedCottonCloth.QuantityMissingInventory);
            
            //Crafting 2 riveria bed, 2 maple lumber in inventory
            list = new CraftList()
                .AddCraftItem("Riviera Bed", 2);
            sourceStore = new CraftItemSourceStore()
                .AddCharacterSource("Maple Lumber", 2, false);
            list.Update(sourceStore);
            readyMaterialsList = list.GetReadyMaterialsListNamed();
            Assert.AreEqual(2, readyMaterialsList["Maple Lumber"]);
            missingMaterialsList = list.GetMissingMaterialsListNamed();
            Assert.AreEqual(8, missingMaterialsList["Copper Ore"]);
            Assert.AreEqual(4, missingMaterialsList["Tin Ore"]);
            Assert.AreEqual(6, missingMaterialsList["Maple Log"]);
            Assert.AreEqual(8, missingMaterialsList["Cotton Boll"]);
            Assert.AreEqual(4, missingMaterialsList["Bronze Ingot"]);
            Assert.AreEqual(8, missingMaterialsList["Cotton Yarn"]);
            Assert.AreEqual(4, missingMaterialsList["Undyed Cotton Cloth"]);
            
            var mapleLumber = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Maple Lumber");
            Assert.AreEqual(4, mapleLumber.QuantityRequired);
            Assert.AreEqual(2, mapleLumber.QuantityNeeded);
            Assert.AreEqual(0, mapleLumber.QuantityAvailable);
            Assert.AreEqual(2, mapleLumber.QuantityReady);
            Assert.AreEqual(0, mapleLumber.QuantityWillRetrieve);
            Assert.AreEqual(2, mapleLumber.QuantityMissingOverall);
            Assert.AreEqual(2, mapleLumber.QuantityMissingInventory);

            var mapleLog = list.GetFlattenedMaterials().First(c => c.Item.NameString == "Maple Log");
            Assert.AreEqual(12, mapleLog.QuantityRequired);
            Assert.AreEqual(6, mapleLog.QuantityNeeded);
            Assert.AreEqual(0, mapleLog.QuantityReady);
            Assert.AreEqual(0, mapleLog.QuantityAvailable);
            Assert.AreEqual(0, mapleLog.QuantityWillRetrieve);
            Assert.AreEqual(6, mapleLog.QuantityMissingOverall);
            Assert.AreEqual(6, mapleLog.QuantityMissingInventory);

            //Crafting 2 riveria bed, 2 lumber available, 6 logs available , plenty of wind shards, should craft 2, already have 2
            list = new CraftList()
                .AddCraftItem("Riviera Bed", 2);
            sourceStore = new CraftItemSourceStore()
                .AddCharacterSource("Maple Lumber", 2, false)
                .AddCharacterSource("Maple Log", 6, false)
                .AddCharacterSource("Wind Shard", 999, false);
            list.Update(sourceStore);
            readyMaterialsList = list.GetReadyMaterialsListNamed();
            quantityNeededList = list.GetQuantityNeededListNamed();
            missingMaterialsList = list.GetMissingMaterialsListNamed();
            var canCraftList = list.GetQuantityCanCraftListNamed();
            
            Assert.AreEqual(2, readyMaterialsList["Maple Lumber"]);
            Assert.AreEqual(2, missingMaterialsList["Maple Lumber"]);
            Assert.AreEqual(0, quantityNeededList["Maple Log"]);
            Assert.AreEqual(0, missingMaterialsList["Maple Log"]);
            Assert.AreEqual(2, canCraftList["Maple Lumber"]);
        }

        [Test]
        public void TestQuantityReadyCalculation()
        {
            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var undyedCottonCloth = new CraftItemSource(5325, 4, false);
            characterMaterials.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});
            
            CraftList list = new CraftList();
            //Rivera Bed
            list.AddCraftItem(6542, 2);
            list.Update(characterMaterials, externalSources);

            var requiredMaterialsList = list.GetReadyMaterialsList();
            //undyed cotton cloth
            Assert.AreEqual(4, requiredMaterialsList[5325]);
        }

        [Test]
        public void TestRetrievalCalcuation()
        {
            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var undyedCottonCloth = new CraftItemSource(5325, 6, false);
            externalSources.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});
            
            CraftList list = new CraftList();
            //Rivera Bed
            list.AddCraftItem(6542, 2);
            list.Update(characterMaterials, externalSources);

            var retrieveList = list.GetQuantityToRetrieveList();
            //undyed cotton cloth
            Assert.AreEqual(4, retrieveList[(5325,false)]);
        }

        //When splitting the stacks of an item you have enough of(say 21 out of 20), it'll appear to want negative of the items required to craft it
        [Test]
        public void TestSplitStacks()
        {
            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var copperIngots = new CraftItemSource(5062, 21, true);
            characterMaterials.Add(5062, new List<CraftItemSource>() {copperIngots});
            
            CraftList list = new CraftList();
            //Glade Drawer Table
            list.AddCraftItem(6624, 10);
            list.GenerateCraftChildren();
            list.Update(characterMaterials, externalSources);

            var flattenedMergedMaterials = list.GetFlattenedMergedMaterials();
            //Works normally
            Assert.AreEqual(false, flattenedMergedMaterials.Any(c => c.ItemId == 5106 && c.QuantityNeeded != 0));
            

            
            //Checking to make sure the amount of copper ore required is 0, not -3
            characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            externalSources = new Dictionary<uint, List<CraftItemSource>>();
            copperIngots = new CraftItemSource(5062, 20, true);
            var copperIngots2 = new CraftItemSource(5062, 1, true);
            characterMaterials.Add(5062, new List<CraftItemSource>() {copperIngots, copperIngots2});
            
            list.GenerateCraftChildren();
            list.Update(characterMaterials, externalSources);
            flattenedMergedMaterials = list.GetFlattenedMergedMaterials();
            Assert.AreEqual(false, flattenedMergedMaterials.Any(c => c.ItemId == 5106 && c.QuantityNeeded != 0));

        }
    }
}