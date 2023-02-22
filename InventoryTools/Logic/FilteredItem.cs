using CriticalCommonLib.Models;

namespace InventoryTools.Logic;

public class FilteredItem
{
    private InventoryItem _item;
    private uint? _quantityRequired;

    public InventoryItem Item => _item;

    public uint? QuantityRequired => _quantityRequired;

    public FilteredItem(InventoryItem item, uint? quantityRequired = null)
    {
        _item = item;
        _quantityRequired = quantityRequired;
    }
}