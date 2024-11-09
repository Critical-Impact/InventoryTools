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
            var excelCache = Host.Services.GetRequiredService<ExcelCache>();

            //Aged Eye of Fire
            var agedEyeFire = excelCache.GetItemSheet().GetRow(9522)!;
            var sources = agedEyeFire.Sources;
            //Not the same as garland tools because we don't deal with individual nodes
            Assert.AreEqual(1, sources.Count);
        }
    }
}