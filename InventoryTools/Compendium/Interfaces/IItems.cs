using System.Collections.Generic;
using AllaganLib.GameSheets.Model;

namespace InventoryTools.Compendium.Interfaces;

public interface IItems
{
    public List<ItemInfo>? GetItems(uint rowId);
    public bool AllowTryOn { get; }
}