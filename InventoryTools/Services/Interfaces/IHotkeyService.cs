using System;
using InventoryTools.Hotkeys;

namespace InventoryTools.Services.Interfaces;

public interface IHotkeyService : IDisposable
{
    void AddHotkey<T>() where T : IHotkey, new();
    void AddHotkey<T>(T instance) where T : IHotkey;
}