#if DEBUG
using System.Collections.Generic;
using System.Numerics;
using Autofac;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.DebugWindows;

public class DebugOverlayServiceWindow : GenericWindow
{
    private readonly IComponentContext _componentContext;
    private IOverlayService? _overlayService;
    
    public IOverlayService OverlayService => _overlayService ??= _componentContext.Resolve<IOverlayService>();

    public DebugOverlayServiceWindow(ILogger<DebugOverlayServiceWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IComponentContext componentContext, string name = "Overlay Service - Debug") : base(logger, mediator, imGuiService, configuration, name)
    {
        _componentContext = componentContext;
    }

    public override void Draw()
    {
        ImGui.Text($"Current State: {(OverlayService.LastState == null ? "No State" : "Has State")}");
        if (OverlayService.LastState != null)
        {
            ImGui.Text($"Filter: {OverlayService.LastState.FilterConfiguration.NameFilter}");
            ImGui.Text($"Should Highlight: {(OverlayService.LastState.ShouldHighlight ? "Yes" : "No")}");
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
        
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => "DebugOverlayService";
    public override string GenericName => "Overlay Service - Debug";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new Vector2(500, 500);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;
    public override void Initialize()
    {
    }
}
#endif