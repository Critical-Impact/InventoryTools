using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.Shared.Interfaces;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Ui.DebugWindows;

public class OverlayServiceDebuggerPane : IDebugPane
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IGameUiManager _gameUiManager;
    private readonly Lazy<IOverlayService> _overlayService;

    public IOverlayService OverlayService => _overlayService.Value;

    public OverlayServiceDebuggerPane(ICharacterMonitor characterMonitor, IGameUiManager gameUiManager, Lazy<IOverlayService> overlayService)
    {
        _characterMonitor = characterMonitor;
        _gameUiManager = gameUiManager;
        _overlayService = overlayService;
    }

    public string Name => "Overlay System";

    public void Draw()
    {
        ImGui.Text($"Current State: {(OverlayService.LastState == null ? "No State" : "Has State")}");
        if (OverlayService.LastState != null)
        {
            ImGui.Text($"Filter: {OverlayService.LastState.FilterConfiguration.NameFilter}");
            ImGui.Text($"Should Highlight: {(OverlayService.LastState.ShouldHighlight ? "Yes" : "No")}");
            ImGui.TextUnformatted($"Active Retainer ID: {_characterMonitor.ActiveRetainerId}");
            ImGui.TextUnformatted($"Retainer List Open?: {_gameUiManager.IsWindowVisible(CriticalCommonLib.Services.Ui.WindowName.RetainerList)}");
            ImGui.Text($"Should Highlight Destination: {(OverlayService.LastState.ShouldHighlightDestination ? "Yes" : "No")}");
            ImGui.Text($"Invert Highlighting: {(OverlayService.LastState.InvertHighlighting ? "Yes" : "No")}");
            ImGui.Text($"Has Filter Result: {(OverlayService.LastState.HasFilterResult ? "Yes" : "No")}");

            var retainerBags1 = OverlayService.LastState.GetBagHighlights(InventoryType.RetainerBag0);
            var retainerBags2 = OverlayService.LastState.GetBagHighlights(InventoryType.RetainerBag1);
            var tabHighlights = OverlayService.LastState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>()
                { retainerBags1, retainerBags2 });
            ImGui.Text($"{(tabHighlights.HasValue ? "Will Highlight Tab 1" : "No Highlight")}");

            var retainerBags3 = OverlayService.LastState.GetBagHighlights(InventoryType.RetainerBag2);
            var retainerBags4 = OverlayService.LastState.GetBagHighlights(InventoryType.RetainerBag3);
            var tabHighlights2 = OverlayService.LastState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>()
                { retainerBags3, retainerBags4 });
            ImGui.Text($"{(tabHighlights2.HasValue ? "Will Highlight Tab 2" : "No Highlight")}");
        }

        ImGui.Text("Overlays: ");
        foreach (var overlay in OverlayService.Overlays)
        {
            ImGui.Text($"{overlay.GetType()}");
            ImGui.Text($"Needs State Refresh: {(overlay.NeedsStateRefresh ? "Yes" : "No")}");
            ImGui.Text($"Should Draw: {(overlay.ShouldDraw ? "Yes" : "No")}");
        }

        if (ImGui.CollapsingHeader("Current State:") && OverlayService.LastState != null)
        {
            Utils.PrintOutObject(OverlayService.LastState, 0, new List<string>());
            if (OverlayService.LastState.FilterResult != null)
            {
                foreach (var result in OverlayService.LastState.FilterResult)
                {
                    Utils.PrintOutObject(result, 0, new List<string>());
                }
            }
        }

    }
}
