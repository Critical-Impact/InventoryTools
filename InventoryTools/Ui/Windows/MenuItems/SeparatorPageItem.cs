using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
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

    public string Name => "Separator";

    public List<MessageBase>? Draw()
    {
        if (_headerName != null)
        {
            if (_includeNewLine)
            {
                ImGui.NewLine();
            }

            ImGui.TextUnformatted(_headerName);
        }
        ImGui.Separator();
        return null;
    }

    public bool IsMenuItem => true;
    public IEnumerable<Page>? ChildPages { get; set; } = null;
    public bool DrawBorder { get; }
}