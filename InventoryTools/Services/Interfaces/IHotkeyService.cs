using System;
using InventoryTools.Hotkeys;

namespace InventoryTools.Services.Interfaces;

public interface IHotkeyService : IDisposable
{
    void AddHotkey(Hotkey hotkey);
}