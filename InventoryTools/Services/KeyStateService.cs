using Dalamud.Game.ClientState.Keys;

namespace InventoryTools.Services;

public class KeyStateService : IKeyStateService
{
    private KeyState _keyState;
    public KeyStateService(KeyState keyState)
    {
        _keyState = keyState;
    }
    public unsafe bool this[int vkCode]
    {
        get => _keyState[vkCode];
        set => _keyState[vkCode] = value;
    }

    public bool this[VirtualKey vkCode]
    {
        get => _keyState[vkCode];
        set => _keyState[vkCode] = value;
    }

    public VirtualKey[] GetValidVirtualKeys()
    {
        return _keyState.GetValidVirtualKeys();
    }

    public void ClearAll()
    {
        _keyState.ClearAll();
    }
}