using System.Collections.Generic;
using AllaganLib.Monitors.Enums;
using AllaganLib.Monitors.Interfaces;
using CriticalCommonLib;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace InventoryTools.Debuggers;

public class AcquisitionDebuggerPane : DebugLogPane
{
    private readonly IAcquisitionMonitorService _acquisitionMonitorService;

    public AcquisitionDebuggerPane(IAcquisitionMonitorService acquisitionMonitorService)
    {
        _acquisitionMonitorService = acquisitionMonitorService;
    }

    public override string Name => "Item Acquisition Monitor";

    public override void SubscribeToEvents()
    {
        _acquisitionMonitorService.ItemAcquired += OnItemAcquired;
        RegisterSubscription(() => _acquisitionMonitorService.ItemAcquired -= OnItemAcquired);
    }

    private void OnItemAcquired(
        uint itemId,
        InventoryItem.ItemFlags itemFlags,
        int qtyIncrease,
        AcquisitionReason reason)
    {
        AddLog(
            $"Item acquired: ItemId={itemId}, " +
            $"QtyChange={qtyIncrease}, " +
            $"Flags={itemFlags}, " +
            $"Reason={reason}"
        );
    }

    public override void DrawInfo()
    {
        if (ImGui.CollapsingHeader("Configuration"))
        {
            var config = _acquisitionMonitorService.Configuration;

            if (config == null)
            {
                ImGui.TextUnformatted("<no configuration>");
            }
            else
            {
                // Intentionally generic: avoids assumptions about configuration shape
                Utils.PrintOutObject(config, 0, new List<string>());
            }
        }

        if (ImGui.CollapsingHeader("Recent Activity"))
        {
            ImGui.TextUnformatted(
                "See the log pane below for a chronological list of acquisition events."
            );
        }
    }
}