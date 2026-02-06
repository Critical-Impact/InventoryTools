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
            Assert.AreEqual(2, sources.Count);
            Assert.AreEqual(false, uplandWheatFlour.ObtainedGathering);
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