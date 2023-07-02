using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public interface IHotkey
{
    public ModifiableHotkey? ModifiableHotkey { get; }
    public bool OnHotKey();

    public bool PassToGame { get; }
}