using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using InventoryToolsTesting.Tests.Abstract;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests
{
    [TestFixture]
    public class CraftTests : BaseTest
    {
        [Test]
        public void TestSkybuildersCalculations()
        {
            var craftListFactory = GetCraftListFactory();

            CraftList list = craftListFactory.Invoke();
            list.AddCraftItem(31922, 2);
            list.GenerateCraftChildren();
            var requiredMaterialsList = list.GetRequiredMaterialsList();
            Assert.AreEqual( 10, requiredMaterialsList[32014]);

            list = craftListFactory.Invoke();
            //Skybuilders bed
            list.AddCraftItem(31945, 1);
            list.GenerateCraftChildren();
            requiredMaterialsList = list.GetRequiredMaterialsList();
            Assert.AreEqual( 10, requiredMaterialsList[32028]);
        }

        [Test]
        public void TestYields()
        {
            var craftListFactory = GetCraftListFactory();

            {
                CraftList list = craftListFactory.Invoke();
                list.AddCraftItem("Cotton Yarn", 3);
                list.GenerateCraftChildren();
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
                CraftList list = craftListFactory.Invoke();
                list.AddCraftItem("Cotton Yarn", 3);
                list.GenerateCraftChildren();
                var requiredMaterialsList = list.GetRequiredMaterialsListNamed();
                Assert.AreEqual(4, requiredMaterialsList["Cotton Boll"]);
                var craftListConfiguration = new CraftListConfiguration()
                    .AddCharacterSource(GetItemIdByName("Cotton Boll"), 4, false)
                    .AddCharacterSource(GetItemIdByName("Lightning Shard"), 4, false);
                list.Update(craftListConfiguration);

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
                Assert.AreEqual(4, cottonYarn.QuantityCanCraft);
            }
        }

        [Test]
        public void TestLeftovers()
        {
            var craftListFactory = GetCraftListFactory();

            //Testing how partial used ingredients of a craft can then be used in crafts in the same list
            CraftList list = craftListFactory.Invoke();
            list.AddCraftItem(38930, 300);
            list.AddCraftItem(38929, 300);
            list.GenerateCraftChildren();
            var requiredMaterialsList = list.GetQuantityNeededList();
            Assert.AreEqual( 600, requiredMaterialsList[36085]);
        }

        [Test]
        public void TestRecipeCalculation()
        {
            var craftListFactory = GetCraftListFactory();

            {
                CraftList list = craftListFactory.Invoke();
                //Rivera Bed
                list.AddCraftItem(6542, 2);
                list.GenerateCraftChildren();
                var requiredMaterialsList = list.GetRequiredMaterialsList();
                //undyed cotton cloth
                Assert.AreEqual(4, requiredMaterialsList[5325]);
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

                list = craftListFactory.Invoke();
                list.AddCraftItem(27877, 1);
                list.GenerateCraftChildren();
                requiredMaterialsList = list.GetRequiredMaterialsList();
                Assert.AreEqual(2, requiredMaterialsList[27830]);
            }

            {
                CraftList list = craftListFactory.Invoke();
                list.AddCraftItem("Dhalmelskin Vest", 6);
                list.AddCraftItem("Ramie Turban of Crafting", 6);
                list.GenerateCraftChildren();

                var neededMaterials = list.GetQuantityNeededListNamed();

                Assert.AreEqual(36, neededMaterials["Dhalmel Hide"]);
                Assert.AreEqual(30, neededMaterials["Hardsilver Sand"]);
                Assert.AreEqual(90, neededMaterials["Stalk of Ramie"]);
                Assert.AreEqual(12, neededMaterials["Dark Chestnut Log"]);
                Assert.AreEqual(18, neededMaterials["Raw Star Sapphire"]);
                Assert.AreEqual(18, neededMaterials["Gold Ore"]);
                Assert.AreEqual(6, neededMaterials["Basilisk Egg"]);
                Assert.AreEqual(6, neededMaterials["Silver Ore"]);
                Assert.AreEqual(84, neededMaterials["Flax"]);
                Assert.AreEqual(6, neededMaterials["Basilisk Whetstone"]);
                Assert.AreEqual(6, neededMaterials["Rose Gold Nugget"]);
                Assert.AreEqual(6, neededMaterials["Hardsilver Nugget"]);
                Assert.AreEqual(6, neededMaterials["Star Sapphire"]);
                Assert.AreEqual(12, neededMaterials["Dhalmel Leather"]);
                Assert.AreEqual(24, neededMaterials["Linen Yarn"]);
                Assert.AreEqual(24, neededMaterials["Ramie Cloth"]);
                Assert.AreEqual(60, neededMaterials["Ramie Thread"]);
            }
        }

        [Test]
        public void TestQuantityAvailableCalculation()
        {
            var craftListFactory = GetCraftListFactory();

            CraftList list;
            CraftListConfiguration sourceStore;
            Dictionary<string, uint> availableMaterialsList;
            Dictionary<string, uint> missingMaterialsList;
            Dictionary<string, uint> readyMaterialsList;
            Dictionary<string, uint> quantityNeededList;

            //Check the amount of available materials is 4 when provided with 4 externally
            list = craftListFactory.Invoke()
                .AddCraftItem("Riviera Bed", 2);
            list.GenerateCraftChildren();
            sourceStore = new CraftListConfiguration()
                .AddExternalSource(GetItemIdByName("Undyed Cotton Cloth"), 4, false);
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
            list = craftListFactory.Invoke()
                .AddCraftItem("Riviera Bed", 2);
            list.GenerateCraftChildren();
            sourceStore = new CraftListConfiguration()
                .AddExternalSource(GetItemIdByName("Undyed Cotton Cloth"), 2, false);
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
            list = craftListFactory.Invoke()
                .AddCraftItem("Riviera Bed", 2);
            list.GenerateCraftChildren();
            sourceStore = new CraftListConfiguration()
                .AddCharacterSource(GetItemIdByName("Maple Lumber"), 2, false);
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
            list = craftListFactory.Invoke()
                .AddCraftItem("Riviera Bed", 2);
            list.GenerateCraftChildren();
            sourceStore = new CraftListConfiguration()
                .AddCharacterSource(GetItemIdByName("Maple Lumber"), 2, false)
                .AddCharacterSource(GetItemIdByName("Maple Log"), 6, false)
                .AddCharacterSource(GetItemIdByName("Wind Shard"), 999, false);
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
            var craftListFactory = GetCraftListFactory();

            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var undyedCottonCloth = new CraftItemSource(5325, 4, false);
            characterMaterials.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});

            CraftList list = craftListFactory.Invoke();
            //Rivera Bed
            list.AddCraftItem(6542, 2);
            list.GenerateCraftChildren();
            list.Update(new CraftListConfiguration(characterMaterials, externalSources));

            var requiredMaterialsList = list.GetReadyMaterialsList();
            //undyed cotton cloth
            Assert.AreEqual(4, requiredMaterialsList[5325]);
        }

        [Test]
        public void TestRetrievalCalcuation()
        {
            var craftListFactory = GetCraftListFactory();

            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var undyedCottonCloth = new CraftItemSource(5325, 6, false);
            externalSources.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});

            CraftList list = craftListFactory.Invoke();
            //Rivera Bed
            list.AddCraftItem(6542, 2);
            list.GenerateCraftChildren();
            list.Update(new CraftListConfiguration(characterMaterials, externalSources));

            var retrieveList = list.GetQuantityToRetrieveList();
            //undyed cotton cloth
            Assert.AreEqual(4, retrieveList[(5325,false)]);
        }

        //When splitting the stacks of an item you have enough of(say 21 out of 20), it'll appear to want negative of the items required to craft it
        [Test]
        public void TestSplitStacks()
        {
            var craftListFactory = GetCraftListFactory();

            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var copperIngots = new CraftItemSource(5062, 21, true);
            characterMaterials.Add(5062, new List<CraftItemSource>() {copperIngots});

            CraftList list = craftListFactory.Invoke();
            //Glade Drawer Table
            list.AddCraftItem(6624, 10);
            list.GenerateCraftChildren();
            list.Update(new CraftListConfiguration(characterMaterials, externalSources));

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
            list.Update(new CraftListConfiguration(characterMaterials, externalSources));
            flattenedMergedMaterials = list.GetFlattenedMergedMaterials();
            Assert.AreEqual(false, flattenedMergedMaterials.Any(c => c.ItemId == 5106 && c.QuantityNeeded != 0));
        }

        [Test]
        public void TestMissingIngredients()
        {
            var craftListFactory = GetCraftListFactory();

            CraftList list = craftListFactory.Invoke();
            CraftListConfiguration store = new CraftListConfiguration();
            list.AddCraftItem("Shark-class Bow", 1);
            list.GenerateCraftChildren();
            list.Update(store);
            var flattenedMergedMaterials = list.GetFlattenedMergedMaterials();
            var venture = flattenedMergedMaterials.First(c => c.ItemId == 5530);
            Assert.AreEqual(20,venture.MissingIngredients.First().Key.Item1);
            Assert.AreEqual(21600,venture.MissingIngredients.First().Value);
        }
    }
}