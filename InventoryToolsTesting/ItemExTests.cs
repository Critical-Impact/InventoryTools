using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using NUnit.Framework;

namespace InventoryToolsTesting
{
    [TestFixture]

    public class ItemExTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
            Service.ExcelCache = new ExcelCache(lumina);
        }
        
        [Test]
        public void TestGatheringSources()
        {
            //Earth Shard
            var earthShard = Service.ExcelCache.GetItemExSheet().GetRow(5)!;
            var gatheringSources = earthShard.GetGatheringSources();
            //Not the same as garland tools because we don't deal with individual nodes
            Assert.AreEqual(16, gatheringSources.Count);
        }

        
        [Test]
        public void TestUplandWheatFlour()
        {
            var uplandWheatFlour = Service.ExcelCache.GetItemExSheet().GetRow(27841)!;
            var gatheringSources = uplandWheatFlour.GetGatheringSources();
            Assert.AreEqual(0, gatheringSources.Count);
            var sources = uplandWheatFlour.Sources;
            Assert.AreEqual(0, sources.Count);
            Assert.AreEqual(false, uplandWheatFlour.ObtainedGathering);
        }

        [Test]
        public void TestScrip()
        {
            //Handsaint Jacket
            var handsaintJacket = Service.ExcelCache.GetItemExSheet().GetRow(31794)!;
            Assert.AreEqual(2, handsaintJacket.Sources.Count);
            Assert.AreEqual(4, handsaintJacket.Vendors.Count);
            var actualVendors = handsaintJacket.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();

            Assert.AreEqual(26, actualVendors.Count);
            
            //Wool Top 16906
            var woolTop = Service.ExcelCache.GetItemExSheet().GetRow(16906)!;
            Assert.AreEqual(2, woolTop.Sources.Count);
            Assert.AreEqual(4, woolTop.Vendors.Count);
            actualVendors = woolTop.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();
            
            Assert.AreEqual(52, actualVendors.Count);
        }

        [Test]
        public void TestTomestones()
        {
            //Palebloom Kudzu Cloth
            var item = Service.ExcelCache.GetItemExSheet().GetRow(37829)!;
            Assert.AreEqual(1, item.Sources.Count);
            
        }

        [Test]
        public void TestMoonwardGear()
        {
            //Moonward Longsword
            var item = Service.ExcelCache.GetItemExSheet().GetRow(34850)!;
            Assert.AreEqual(2, item.Sources.Count);
            
        }

    }
}