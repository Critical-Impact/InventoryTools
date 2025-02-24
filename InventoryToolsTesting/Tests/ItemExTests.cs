using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests
{
    [TestFixture]

    public class ItemRowTests : BaseTest
    {
        [Test]
        public void TestGatheringSources()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();
            //Earth Shard
            var earthShard = itemSheet.GetRow(5)!;
            var gatheringSources = earthShard.GetSourcesByCategory<ItemGatheringSource>(ItemInfoCategory.Gathering);
            Assert.AreEqual(4, gatheringSources.Count);
        }


        [Test]
        public void TestUplandWheatFlour()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();
            var uplandWheatFlour = itemSheet.GetRow(27841)!;
            var gatheringSources = uplandWheatFlour.GetSourcesByCategory<ItemGatheringSource>(ItemInfoCategory.Gathering);
            Assert.AreEqual(0, gatheringSources.Count);
            var sources = uplandWheatFlour.Sources;
            Assert.AreEqual(1, sources.Count);
            Assert.AreEqual(false, uplandWheatFlour.ObtainedGathering);
        }

        [Test]
        public void TestScrip()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();
            //Handsaint Jacket
            var handsaintJacket = itemSheet.GetRow(31794)!;
            Assert.AreEqual(3, handsaintJacket.Sources.Count);
            var shops = handsaintJacket.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop);
            Assert.AreEqual(3, shops.Count);
            var actualVendors = shops.SelectMany(shop => shop.Shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();

            Assert.AreEqual(18, actualVendors.Count);

            //Wool Top 16906
            var woolTop = itemSheet.GetRow(16906)!;
            Assert.AreEqual(7, woolTop.Sources.Count);
            shops = woolTop.GetSourcesByCategory<ItemShopSource>(ItemInfoCategory.Shop);
            Assert.AreEqual(4, shops.Count);
            actualVendors = shops.SelectMany(shop => shop.Shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();

            Assert.AreEqual(36, actualVendors.Count);
        }

        [Test]
        public void TestTomestones()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();
            //Palebloom Kudzu Cloth
            var item = itemSheet.GetRow(37829)!;
            Assert.AreEqual(2, item.Sources.Count);

        }

        [Test]
        public void TestMoonwardGear()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();
            //Moonward Longsword
            var item = itemSheet.GetRow(34850)!;
            Assert.AreEqual(2, item.Sources.Count);

        }

    }
}