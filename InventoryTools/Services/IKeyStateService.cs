using Dalamud.Game.ClientState.Keys;

namespace InventoryTools.Services;

public interface IKeyStateService
{
    unsafe bool this[int vkCode] { get; set; }

    bool this[VirtualKey vkCode] { get; set; }

    VirtualKey[] GetValidVirtualKeys();

    void ClearAll();
}