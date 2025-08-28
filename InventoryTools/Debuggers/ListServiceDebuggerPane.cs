using System;
using AllaganLib.Shared.Interfaces;
using Dalamud.Bindings.ImGui;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Ui.DebugWindows;

public class ListServiceDebuggerPane : IDebugPane
{
    private Lazy<IListService> _listService;

    public IListService ListService => _listService.Value;

    public ListServiceDebuggerPane(Lazy<IListService> listService)
    {
        _listService = listService;
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
            ImGui.Text($"{list.Name}:");
            ImGui.Text($"{(list.Active ? "Active" : "Not Active")}");
        }
    }
}
