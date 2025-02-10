using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace InventoryToolsMock;

public class MockGameUiManager : IGameUiManager
{
    public void Dispose()
    {
    }

    public void ManualInvokeUiVisibilityChanged(WindowName windowName, bool status)
    {
        UiVisibilityChanged?.Invoke(windowName, status);
    }

    public event GameUiManager.UiVisibilityChangedDelegate? UiVisibilityChanged;
    public event GameUiManager.UiUpdatedDelegate? UiUpdated;
    public bool IsWindowVisible(WindowName windowName)
    {
        return false;
    }

    public bool WatchWindowState(WindowName windowName)
    {
        return false;
    }

    public unsafe AtkUnitBase* GetWindow(string windowName)
    {
        return null;
    }

    public nint GetWindowAsPtr(string windowName)
    {
        unsafe
        {
            return new nint(null);
        }
    }

    public bool IsWindowLoaded(WindowName windowName)
    {
        return false;
    }

    public bool IsWindowFocused(WindowName windowName)
    {
        return false;
    }

    public bool IsWindowFocused(string windowName)
    {
        return false;
    }

    public unsafe bool TryGetAddonByName<T>(string Addon, out T* AddonPtr) where T : unmanaged
    {
        AddonPtr = null;
        return false;
    }

    public unsafe T* GetNodeByID<T>(AtkUldManager uldManager, uint nodeId, NodeType? type = null) where T : unmanaged
    {
        return null;
    }
}