using System.Collections.Generic;

namespace InventoryTools.Extensions
{
    public static class ListExtensions
    {
        public static List<T> MoveUp<T>(this List<T> items, T item)
        {
            var oldIndex = items.IndexOf(item);
            if (oldIndex > 0)
            {
                items.RemoveAt(oldIndex);
                oldIndex--;
                items.Insert(oldIndex, item);
            }
            return items;
        }
        public static List<T> MoveDown<T>(this List<T> items, T item)
        {
            var oldIndex = items.IndexOf(item);
            if (oldIndex + 1 < items.Count)
            {
                items.RemoveAt(oldIndex);
                oldIndex++;
                items.Insert(oldIndex, item);
            }

            return items;
        }
    }
}