using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Plugin.Services;

namespace InventoryToolsTesting.Services;

public class TestInventoryMonitor : InventoryMonitor
{
    public TestInventoryMonitor(ICharacterMonitor monitor, ICraftMonitor craftMonitor, IInventoryScanner scanner, IFramework frameworkService, ExcelCache excelCache) : base(monitor, craftMonitor, scanner, frameworkService, excelCache)
    {
    }

    public void AddInventory(Inventory inventory)
    {
        this.Inventories[inventory.CharacterId] = inventory;
    }

    public void AddInventory(IEnumerable<Inventory> inventories)
    {
        foreach (var inventory in inventories)
        {
            AddInventory(inventory);
        }
    }
}