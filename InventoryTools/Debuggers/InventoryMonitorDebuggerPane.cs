using System;
using System.Collections.Generic;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Debuggers;

public class InventoryMonitorDebuggerPane : IDebugPane
{
    private readonly IInventoryMonitor _inventoryMonitor;

    public InventoryMonitorDebuggerPane(IInventoryMonitor inventoryMonitor)
    {
        _inventoryMonitor = inventoryMonitor;
    }
    public string Name => "Inventory Monitor";
    public void Draw()
    {
        foreach (var character in _inventoryMonitor.Inventories)
        {
            using (var characterNode = ImRaii.TreeNode(character.Key + "##" + character.Key))
            {
                if (characterNode.Success)
                {
                    using (ImRaii.PushId(character.Key.ToString()))
                    {
                        var possibleValues = Enum.GetValues<CriticalCommonLib.Enums.InventoryType>();
                        foreach (var possibleValue in possibleValues)
                        {
                            var bag = character.Value.GetInventoryByType(possibleValue);
                            var bagName = possibleValue.ToString();
                            if (bag != null)
                            {
                                using (var bagNode = ImRaii.TreeNode(bagName + "##" + bagName))
                                {
                                    if (bagNode.Success)
                                    {
                                        for (int i = 0; i < bag.Length; i++)
                                        {
                                            var item = bag[i];
                                            if (item != null)
                                            {
                                                Utils.PrintOutObject(item, (ulong)i,
                                                    new List<string>());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}