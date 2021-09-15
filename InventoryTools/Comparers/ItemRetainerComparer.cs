using System.Collections;
using System.Collections.Generic;
using CriticalCommonLib.Models;

namespace InventoryTools.Comparers
{
    public class ItemRetainerComparer : IEqualityComparer<InventoryItem>
    {
        public bool Equals(InventoryItem x, InventoryItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.ItemId == y.ItemId && x.IsHQ == y.IsHQ;
        }

        public int GetHashCode(InventoryItem obj)
        {
            unchecked
            {
                return ((int) obj.ItemId * 397) ^ obj.IsHQ.GetHashCode();
            }
        }
    }
}