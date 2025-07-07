using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using InventoryToolsTesting.Tests.Abstract;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace InventoryToolsTesting.Tests
{
    [TestFixture]

    public class SupplementalTests : BaseTest
    {
        [Test]
        public void TestDesynthesisSupplements()
        {
            var itemSheet = Host.Services.GetRequiredService<ItemSheet>();

            //Aged Eye of Fire
            var agedEyeFire = itemSheet.GetRow(9522)!;
            var sources = agedEyeFire.Sources;

            Assert.AreEqual(2, sources.Count);
        }
    }
}