using CriticalCommonLib.Services.Mediator;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public interface IHotkey
{
    public ModifiableHotkey? ModifiableHotkey { get; }
    public VirtualKey[]? VirtualKeys { get; }
    public bool OnHotKey();
    public bool PassToGame { get; }
}