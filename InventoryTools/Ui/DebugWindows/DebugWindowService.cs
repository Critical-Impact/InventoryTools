#if DEBUG
using System.Numerics;
using Autofac;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.DebugWindows;

public class DebugWindowServiceWindow : GenericWindow
{
    private readonly IComponentContext _componentContext;
    private WindowService? _windowService;

    public DebugWindowServiceWindow(ILogger<DebugWindowServiceWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IComponentContext componentContext, string name = "Window Service - Debug") : base(logger, mediator, imGuiService, configuration, name)
    {
        _componentContext = componentContext;
        //Works around circular references, find for debugging
    }

    public override void Draw()
    {
        ImGui.Text($"Filter Window Open: {WindowService.HasFilterWindowOpen}");

    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => "DebugWindowService";
    public override string GenericName => "Window Service - Debug";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new Vector2(500, 500);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;

    public WindowService WindowService => _windowService ??= _componentContext.Resolve<WindowService>();

    public override void Initialize()
    {
    }
}
#endif