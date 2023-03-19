using Dalamud.Game.ClientState.Keys;
using InventoryTools.Services.Interfaces;

namespace InventoryToolsMock;

public class MockKeyStateService : IKeyStateService
{
    private HashSet<VirtualKey> _activeKeys { get; } = new HashSet<VirtualKey>();
    public unsafe bool this[int vkCode]
    {
        get => _activeKeys.Contains((VirtualKey)vkCode);
        set
        {
            if (value)
            {
                _activeKeys.Add((VirtualKey)vkCode);
            }
            else
            {
                _activeKeys.Remove((VirtualKey)vkCode);
            }
        }
    }

    public bool this[VirtualKey vkCode]
    {
        get => this[(int)vkCode];
        set => this[(int)vkCode] = value;
    }

    public VirtualKey[] GetValidVirtualKeys() => (VirtualKey[])Enum.GetValuesAsUnderlyingType<VirtualKey>();


    public void ClearAll()
    {
        _activeKeys.Clear();
    }
}