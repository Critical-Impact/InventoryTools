using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Extensions
{
    public static class ItemExtensions
    {
        public static bool CanBeTraded(this Item? item)
        {
            return item is {IsUntradable: false} && item.ItemSearchCategory.Row != 0;
        }
        
        public static string FormattedSearchCategory(this Item item)
        {
            return item.ItemSearchCategory?.Value == null ? "" : item.ItemSearchCategory.Value.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-");
        }

        public static string FormattedUiCategory(this Item item)
        {
            return item.ItemUICategory?.Value == null ? "" : item.ItemUICategory.Value.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-");
        }
    }
}