using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using NUnit.Framework;

namespace InventoryToolsTesting
{
    [TestFixture]

    public class SupplementalTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
            Service.ExcelCache = new ExcelCache(lumina);
        }
        
        [Test]
        public void TestDesynthesisSupplements()
        {
            //Aged Eye of Fire
            var agedEyeFire = Service.ExcelCache.GetSheet<ItemEx>().GetRow(9522)!;
            var sources = agedEyeFire.Sources;
            //Not the same as garland tools because we don't deal with individual nodes
            Assert.AreEqual(1, sources.Count);
        }
    }
}