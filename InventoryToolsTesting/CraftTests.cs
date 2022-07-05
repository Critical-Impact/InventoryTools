using System.Collections.Generic;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;
using NUnit.Framework;

namespace TestProject1
{
    [TestFixture]
    public class CraftTests
    {

        [SetUp]
        public void Init()
        {
            var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
            Service.ExcelCache = new ExcelCache(lumina);
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
            var availableMaterialsList = list.GetAvailableMaterialsList();
            var merged = list.GetFlattenedMergedMaterials();
            var a = "";
        }

        [Test]
        public void TestQuantityAvailableCalculation()
        {
            var characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            var externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var undyedCottonCloth = new CraftItemSource(5325, 4, false);
            externalSources.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});
            
            CraftList list = new CraftList();
            //Rivera Bed
            list.AddCraftItem(6542, 2);
            list.Update(characterMaterials, externalSources);

            var availableMaterialsList = list.GetAvailableMaterialsList();
            //undyed cotton cloth
            Assert.AreEqual(4, availableMaterialsList[5325]);
            
            
            characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            externalSources = new Dictionary<uint, List<CraftItemSource>>();
            undyedCottonCloth = new CraftItemSource(5325, 2, false);
            externalSources.Add(5325, new List<CraftItemSource>() {undyedCottonCloth});
            list.Update(characterMaterials, externalSources);
            availableMaterialsList = list.GetAvailableMaterialsList();
            //undyed cotton cloth
            Assert.AreEqual(2, availableMaterialsList[5325]);

            var missingMaterialsList = list.GetMissingMaterialsList();
            Assert.AreEqual(4, missingMaterialsList[5334]);
            Assert.AreEqual(4, missingMaterialsList[5343]);
            
            //Maple lumber
            characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            externalSources = new Dictionary<uint, List<CraftItemSource>>();
            var mapleLumber = new CraftItemSource(5361, 2, false);
            characterMaterials.Add(5361, new List<CraftItemSource>() {mapleLumber});
            list.Update(characterMaterials, externalSources);
            var readyMaterialsList = list.GetReadyMaterialsList();
            Assert.AreEqual(2, readyMaterialsList[5361]);

            var quantityNeededList = list.GetQuantityNeededList();
            missingMaterialsList = list.GetMissingMaterialsList();
            Assert.AreEqual(2, missingMaterialsList[5361]);
            Assert.AreEqual(6, quantityNeededList[5380]);
            Assert.AreEqual(6, missingMaterialsList[5380]);
            
            //Maple lumber
            characterMaterials = new Dictionary<uint, List<CraftItemSource>>();
            externalSources = new Dictionary<uint, List<CraftItemSource>>();
            mapleLumber = new CraftItemSource(5361, 2, false);
            var mapleLog = new CraftItemSource(5380, 6, false);
            var windShard = new CraftItemSource(4, 999, false);
            characterMaterials.Add(5361, new List<CraftItemSource>() {mapleLumber});
            characterMaterials.Add(5380, new List<CraftItemSource>() {mapleLog});
            characterMaterials.Add(4, new List<CraftItemSource>() {windShard});
            list.Update(characterMaterials, externalSources);
            readyMaterialsList = list.GetReadyMaterialsList();
            quantityNeededList = list.GetQuantityNeededList();
            missingMaterialsList = list.GetMissingMaterialsList();
            var canCraftList = list.GetQuantityCanCraftList();
            Assert.AreEqual(2, readyMaterialsList[5361]);
            Assert.AreEqual(2, missingMaterialsList[5361]);
            Assert.AreEqual(0, quantityNeededList[5380]);
            Assert.AreEqual(0, missingMaterialsList[5380]);
            Assert.AreEqual(2, canCraftList[5361]);
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
            Assert.AreEqual(4, retrieveList[5325]);
        }
    }
}