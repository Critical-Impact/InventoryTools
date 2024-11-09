using System.Linq;
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
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();
            //Earth Shard
            var earthShard = excelCache.GetItemSheet().GetRow(5)!;
            var gatheringSources = earthShard.GetGatheringSources();
            //Not the same as garland tools because we don't deal with individual nodes
            Assert.AreEqual(16, gatheringSources.Count);
        }

        
        [Test]
        public void TestUplandWheatFlour()
        {
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();
            var uplandWheatFlour = excelCache.GetItemSheet().GetRow(27841)!;
            var gatheringSources = uplandWheatFlour.GetGatheringSources();
            Assert.AreEqual(0, gatheringSources.Count);
            var sources = uplandWheatFlour.Sources;
            Assert.AreEqual(0, sources.Count);
            Assert.AreEqual(false, uplandWheatFlour.ObtainedGathering);
        }

        [Test]
        public void TestScrip()
        {
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();
            //Handsaint Jacket
            var handsaintJacket = excelCache.GetItemSheet().GetRow(31794)!;
            Assert.AreEqual(2, handsaintJacket.Sources.Count);
            Assert.AreEqual(4, handsaintJacket.Vendors.Count);
            var actualVendors = handsaintJacket.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();

            Assert.AreEqual(10, actualVendors.Count);
            
            //Wool Top 16906
            var woolTop = excelCache.GetItemSheet().GetRow(16906)!;
            Assert.AreEqual(4, woolTop.Sources.Count);
            Assert.AreEqual(4, woolTop.Vendors.Count);
            actualVendors = woolTop.Vendors.SelectMany(shop => shop.ENpcs.SelectMany(npc => npc.Locations.Select(location => (shop, npc, location)))).ToList();
            
            Assert.AreEqual(28, actualVendors.Count);
        }

        [Test]
        public void TestTomestones()
        {
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();
            //Palebloom Kudzu Cloth
            var item = excelCache.GetItemSheet().GetRow(37829)!;
            Assert.AreEqual(1, item.Sources.Count);
            
        }

        [Test]
        public void TestMoonwardGear()
        {
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();
            //Moonward Longsword
            var item = excelCache.GetItemSheet().GetRow(34850)!;
            Assert.AreEqual(2, item.Sources.Count);
            
        }

    }
}