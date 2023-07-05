using CriticalCommonLib.Interfaces;
using Dalamud.Logging;

namespace InventoryToolsMock;

public class MockTeleporterIpc : ITeleporterIpc
{
    public bool IsAvailable { get; } = true;
    public bool Teleport(uint aetheryteId)
    {
        PluginLog.Log("Would have sent a IPC request to Teleporter to goto " + aetheryteId);
        return true;
    }
}