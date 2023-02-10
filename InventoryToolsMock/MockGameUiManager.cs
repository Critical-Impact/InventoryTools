using CriticalCommonLib.Services.Ui;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace InventoryToolsMock;

public class MockGameUiManager : IGameUiManager
{
    public void Dispose()
    {
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
}