using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using NUnit.Framework;
using CriticalCommonLib.Extensions;

namespace InventoryToolsTesting
{
    [TestFixture]

    public class UtilityTesting
    {
        [OneTimeSetUp]
        public void Init()
        {
            var lumina = new Lumina.GameData( "H:/Games/SquareEnix/FINAL FANTASY XIV - A Realm Reborn/game/sqpack" );
            Service.ExcelCache = new ExcelCache(lumina);
        }
        
        [Test]
        public void TestTeamCraftParsing()
        {
            string craftList = @"
Items :
1x Cedar Lumber
1x Mythrite Trident
1x Cedar Longbow
1x Cedar Crook
1x Holy Cedar Lumber
1x Mythrite Pugil Stick
1x Holy Cedar Composite Bow
1x Cedar Fishing Rod
2x Rarefied Cedar Fishing Rod
1x Astral Grinding Wheel
1x Holy Cedar Spinning Wheel
1x Holy Cedar Armillae
1x Holy Cedar Ring
1x Dark Chestnut Lumber
2x Rarefied Holy Cedar Spinning Wheel
1x Titanium Lance
1x Dark Chestnut Longbow
1x Dark Chestnut Rod
1x Holy Cedar Necklace
1x Dark Chestnut Fishing Rod
1x Hallowed Chestnut Lumber
2x Rarefied Dark Chestnut Rod
1x Titanium Fork
1x Hallowed Chestnut Composite Bow
1x Cloud Mica Grinding Wheel
1x Dark Chestnut Spinning Wheel
1x Hallowed Chestnut Mask of Aiming
1x Hallowed Chestnut Mask of Casting
1x Hallowed Chestnut Mask of Healing
1x Hallowed Chestnut Fishing Rod
1x Hallowed Chestnut Ring
1x Birch Lumber
1x Adamantite Spear
1x Birch Longbow
1x Birch Signum
1x Hallowed Chestnut Armillae
2x Rarefied Hallowed Chestnut Ring
1x Dragonscale Grinding Wheel
1x Birch Spinning Wheel
1x Hallowed Chestnut Necklace
1x Blank Grade 3 Orchestrion Roll
1x Cassia Lumber
1x The Unending Journey
1x Adamantite Trident
1x Birch Composite Bow
1x Birch Rod
1x Birch Fishing Rod
1x Hive Ceiling Fan
1x Cassia Block
2x Dispelling Arrow
1x Clockwork Barrow
1x Unfinished Interior Wall
1x Unfinished Wood Flooring
1x Deep Hive Ceiling Fan
1x Magnificent Mogdelier
1x Bar Stool
1x Gordian Bureau
1x Bar Counter
1x Stuffed Carbuncle
1x Astral Birch Lumber
1x Sun Mica Grinding Wheel
1x Astral Birch Spinning Wheel
1x Astral Birch Necklace
1x Astral Birch Armillae
1x Astral Birch Ring
1x Mhachi Coffin
1x Savage Gordian Bureau
1x Tiny Bronco Miniature
1x Invincible Miniature
1x Enterprise Miniature
1x Invincible II Miniature
1x Odyssey Miniature
1x Tatanora Miniature
1x Viltgance Miniature
1x Camphorwood Lumber
1x Endless Expanse Partisan
1x Dead Hive Spear
1x Endless Expanse Longbow
1x Dead Hive Bow
1x Endless Expanse Cane
1x Dead Hive Cane
1x Shishi-odoshi
1x Orchestrion
1x Pudding Desk
1x Troupe Stage
1x Tier 3 Aquarium
1x Treated Camphorwood Lumber
1x Halberd of the Round
1x Pike of the Fiend
1x Seeing Horde Spear
1x Pike of the Goddess
1x Pike of the Demon
1x Bow of the Round
1x Bow of the Fiend
1x Seeing Horde Bow
1x Bow of the Goddess
1x Bow of the Demon
1x Cane of the Round
1x Cane of the Fiend
1x Camphorwood Necklace of Fending
1x Camphorwood Necklace of Slaying
1x Camphorwood Necklace of Aiming
1x Camphorwood Necklace of Healing
1x Camphorwood Necklace of Casting
1x Camphorwood Armillae of Fending
1x Camphorwood Armillae of Slaying
1x Camphorwood Armillae of Aiming
1x Camphorwood Armillae of Healing
1x Camphorwood Armillae of Casting
1x Oriental Wood Bridge
1x Bread Rack
1x Knightly Round Table
1x Grade 3 Picture Frame
1x Eikon Iron Grinding Wheel
1x Camphorwood Spinning Wheel
1x Luminous Fiber Fishing Rod
1x Ironworks Necklace of Crafting
1x Ironworks Necklace of Gathering
1x Ironworks Armillae of Crafting
1x Ironworks Armillae of Gathering
1x Teak Lumber
1x Heavy Metal Lance
1x Teak Composite Bow
1x Teak Cane
1x Teak Choker of Fending
1x Teak Choker of Slaying
1x Teak Choker of Aiming
1x Teak Choker of Healing
1x Teak Choker of Casting
1x Teak Bracelet of Fending
1x Teak Bracelet of Slaying
1x Teak Bracelet of Aiming
1x Teak Bracelet of Healing
1x Teak Bracelet of Casting
1x Hingan Cleaning Supplies
1x Tier 4 Aquarium
1x Highland Lancet Window
1x Highland Oblong Window
1x Highland Wooden Door
1x Highland Classical Door
1x Highland Wooden Awning
1x Highland Flooring
1x Highland Cottage Roof (Wood)
1x Highland House Roof (Wood)
1x Highland Mansion Roof (Wood)
1x Highland Cottage Wall (Wood)
1x Highland House Wall (Wood)
1x Highland Mansion Wall (Wood)
1x Hat Stand
1x Alpine Chair
1x Round Banquet Table
1x Cask Rack
1x Alpine Round Table
1x Steppe Kitchen
1x Royal Plotting Table
1x War Chest
1x Toy Box
1x Tier 2 Aquarium
1x Paissa Floor Lamp
1x Imitation Shuttered Window
1x Grade 2 Picture Frame
2x Rarefied Birch Signum
";

            var parsedItems = craftList.ParseItemsFromCraftList();
            Assert.AreEqual(162, parsedItems.Count);
            Assert.AreEqual(2, parsedItems.Last().Value);
        }
    }
}