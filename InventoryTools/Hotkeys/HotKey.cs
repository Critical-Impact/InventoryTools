using Dalamud.Game.ClientState.Keys;
using InventoryTools.Extensions;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public abstract class Hotkey : IHotkey
{
    private VirtualKey[]? _keys;
    private ModifiableHotkey? _currentHotkey;
    
    public VirtualKey[]? VirtualKeys
    {
        get
        {
            if (ModifiableHotkey != _currentHotkey)
            {
                _currentHotkey = ModifiableHotkey;
                _keys = null;
            }
            if (_keys != null)
            {
                return _keys;
            }
            if (_keys == null && ModifiableHotkey.HasValue)
            {
                _keys = ModifiableHotkey.Value.VirtualKeys();
                if (_keys.Length == 0)
                {
                    _keys = null;
                }
            }
            return _keys;
        }
    }

    public abstract ModifiableHotkey? ModifiableHotkey { get; }
    public abstract bool OnHotKey();
    public bool PassToGame { get; }
}