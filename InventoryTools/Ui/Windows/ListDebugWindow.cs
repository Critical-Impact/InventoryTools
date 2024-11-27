using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class ListDebugWindow : GenericWindow
{
    private readonly IListService _listService;
    private readonly TableService _tableService;
    private List<FilterConfiguration> _lists;

    public ListDebugWindow(ILogger<ListDebugWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, IListService listService, TableService tableService, string name = "") : base(logger, mediator, imGuiService, configuration, name)
    {
        _listService = listService;
        _tableService = tableService;
    }

    public override string GenericKey { get; } = "listdebug";
    public override string GenericName { get; } = "List Debug";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new(500, 500);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;

    public override void Initialize()
    {
        _lists = _listService.Lists;
    }

    public override void Draw()
    {
        foreach (var list in _lists)
        {
            ImGui.Text("List: " + list.Name);
            ImGui.Text("Refreshing: " + (list.Refreshing ? "Yes" : "No"));
            ImGui.Text("Needs Refresh: " + (list.NeedsRefresh ? "Yes" : "No"));
        }
    }

    public override void Invalidate()
    {
    }

    public override FilterConfiguration? SelectedConfiguration => null;
}