using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Widgets;

public class HoverImageButton
{
    private bool _isHovered;
    private string _id;

    public HoverImageButton(string id)
    {
        _id = id;
    }

    public bool Draw(nint textureId, Vector2 size, int framePadding = 0, Vector2? uv0 = null, Vector2? uv1 = null, Vector4? bgColor = null, Vector4? tintColor = null, Vector4? bgColorHover = null, Vector4? tintColorHover = null)
    {
        bgColorHover ??= new Vector4(1, 1, 1, 0);
        bgColor ??= bgColorHover;

        tintColor ??= new Vector4(1, 1, 1, 1);
        tintColorHover ??= ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered];

        var success = false;
        using var pushId  = ImRaii.PushId(_id);
        if (ImGui.ImageButton(textureId,size * ImGui.GetIO().FontGlobalScale, uv0 ?? new Vector2(0,0), uv1 ?? new Vector2(1,1), framePadding,
                _isHovered ? bgColorHover.Value : bgColor.Value, _isHovered ? tintColorHover.Value : tintColor.Value))
        {
            success = true;
        }

        if (ImGui.IsItemHovered())
        {
            _isHovered = true;
        }
        else
        {
            _isHovered = false;
        }

        return success;
    }
}
