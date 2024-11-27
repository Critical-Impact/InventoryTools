#if DEBUG
using System.Numerics;
using Autofac;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.DebugWindows;

public class DebugListServiceWindow : GenericWindow
{
    private readonly IComponentContext _componentContext;
    private IListService? _listService;
    
    public IListService ListService => _listService ??= _componentContext.Resolve<IListService>();

    public DebugListServiceWindow(ILogger<DebugListServiceWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IComponentContext componentContext, string name = "List Service - Debug") : base(logger, mediator, imGuiService, configuration, name)
    {
        _componentContext = componentContext;
    }

    public override void Draw()
    {
        var activeBackgroundList = ListService.GetActiveBackgroundList();
        var activeUiList = ListService.GetActiveUiList(false);
        var lists = ListService.Lists;
        ImGui.Text($"Active Background List: {(activeBackgroundList == null ? "No List" : activeBackgroundList.NameFilter)}");
        ImGui.Text($"Active UI List: {(activeUiList == null ? "No List" : activeUiList.NameFilter)}");
        foreach (var list in lists)
        {
            ImGui.Text($"{list.NameFilter}:");
            ImGui.Text($"{(list.Active ? "Active" : "Not Active")}");
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => "DebugListService";
    public override string GenericName => "List Service - Debug";
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