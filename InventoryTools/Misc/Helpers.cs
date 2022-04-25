using System.Collections.Generic;

namespace InventoryTools.Misc
{
    public static class Helpers
    {
        public static readonly HashSet<uint> HousingCategoryIds = new() {64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 75, 76, 77, 78, 79, 80, 82, 85, 95, 57}; 
        public static readonly HashSet<uint> CraftingMaterialIds = new() {45, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60}; 
    }
}