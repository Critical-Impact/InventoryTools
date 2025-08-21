using System;
using AllaganLib.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services;

namespace InventoryTools.Debuggers;

public class WindowServiceDebuggerPane : IDebugPane
{
    private readonly Lazy<WindowService> _windowService;

    public WindowService WindowService => _windowService.Value;

    public WindowServiceDebuggerPane(Lazy<WindowService> windowService)
    {
        _windowService = windowService;
    }
    public string Name => "Window Service";
    public void Draw()
    {
        ImGui.Text($"Filter Window Open: {WindowService.HasFilterWindowOpen}");
    }
}