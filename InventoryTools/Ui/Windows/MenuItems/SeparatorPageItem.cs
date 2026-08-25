using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Ui.Pages;

namespace InventoryTools.Ui.MenuItems;

public class SeparatorPageItem : IConfigPage
{
    private string? _headerName;
    private bool _includeNewLine;

    public SeparatorPageItem(string? headerName = null, bool includeNewLine = false)
    {
        _includeNewLine = includeNewLine;
        _headerName = headerName;
    }

    public void Initialize()
    {

    }

    public string Key => "separator/" + (_headerName ?? string.Empty);

    public string Name => "Separator";

    public List<MessageBase>? Draw()
    {
        if (_includeNewLine)
        {
            ImGui.NewLine();
        }

        if (_headerName != null)
        {
            ImGui.TextUnformatted(_headerName);
        }

        ImGui.Separator();
        return null;
    }

    public bool IsMenuItem => true;
    public IEnumerable<IConfigPage>? ChildPages { get; set; } = null;
    public bool DrawBorder { get; }
}