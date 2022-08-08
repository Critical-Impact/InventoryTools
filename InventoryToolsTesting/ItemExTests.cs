using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
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
            var earthShard = Service.ExcelCache.GetSheet<ItemEx>().GetRow(5)!;
            var gatheringSources = earthShard.GetGatheringSources();
            //Not the same as garland tools because we don't deal with individual nodes
            Assert.AreEqual(16, gatheringSources.Count);
        }

        [Test]
        public void TestScrip()
        {
            //Handsaint Jacket
            var handsaintJacket = Service.ExcelCache.GetSheet<ItemEx>().GetRow(31794)!;
            Assert.AreEqual(2, handsaintJacket.Sources.Count);
            Assert.AreEqual(4, handsaintJacket.Vendors.Count);
            var actualVendors = handsaintJacket.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();

            Assert.AreEqual(26, actualVendors.Count);
            
            //Wool Top 16906
            var woolTop = Service.ExcelCache.GetSheet<ItemEx>().GetRow(16906)!;
            Assert.AreEqual(2, woolTop.Sources.Count);
            Assert.AreEqual(10, woolTop.Vendors.Count);
            actualVendors = woolTop.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();
            
            Assert.AreEqual(52, actualVendors.Count);
        }

    }
}