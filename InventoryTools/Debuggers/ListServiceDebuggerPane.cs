using System;
using AllaganLib.Shared.Interfaces;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using OtterGui.Raii;

namespace InventoryTools.Ui.DebugWindows;

public class ListServiceDebuggerPane : IDebugPane
{
    private Lazy<IListService> _listService;
    private readonly MediatorService _mediatorService;

    public IListService ListService => _listService.Value;

    public ListServiceDebuggerPane(Lazy<IListService> listService, MediatorService mediatorService)
    {
        _listService = listService;
        _mediatorService = mediatorService;
    }

    public string Name => "List Service";

    public void Draw()
    {
        var activeBackgroundList = ListService.GetActiveBackgroundList();
        var activeUiList = ListService.GetActiveUiList(false);
        var lists = ListService.Lists;
        ImGui.Text($"Active Background List: {(activeBackgroundList == null ? "No List" : activeBackgroundList.Name)}");
        ImGui.Text($"Active UI List: {(activeUiList == null ? "No List" : activeUiList.Name)}");
        foreach (var list in lists)
        {
            using var id = ImRaii.PushId(list.Key);
            ImGui.Text($"{list.Name}:");
            ImGui.Text($"{(list.Active ? "Active" : "Not Active")}");
            ImGui.SameLine();
            if (ImGui.Button("Request Refresh"))
            {
                _mediatorService.Publish(new RequestListUpdateMessage(list));
            }
        }
    }
}
