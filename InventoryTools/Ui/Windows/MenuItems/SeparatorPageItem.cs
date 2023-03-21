using ImGuiNET;
using InventoryTools.Logic;

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
    public string Name => "Separator";

    public void Draw()
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
    }

    public bool IsMenuItem => true;
}