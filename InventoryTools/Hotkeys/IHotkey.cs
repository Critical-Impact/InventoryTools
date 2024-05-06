using Dalamud.Game.ClientState.Keys;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public interface IHotkey
{
    public ModifiableHotkey? ModifiableHotkey { get; }
    public VirtualKey[]? VirtualKeys { get; }
    public bool OnHotKey();
    public bool PassToGame { get; }
}