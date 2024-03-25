using CriticalCommonLib.Interfaces;
using Dalamud.Plugin.Services;

namespace InventoryToolsMock;

public class MockTeleporterIpc : ITeleporterIpc
{
    private readonly IPluginLog _pluginLog;

    public MockTeleporterIpc(IPluginLog pluginLog)
    {
        _pluginLog = pluginLog;
    }
    
    public bool IsAvailable { get; } = true;
    public bool Teleport(uint aetheryteId)
    {
        _pluginLog.Info("Would have sent a IPC request to Teleporter to goto " + aetheryteId);
        return true;
    }
}