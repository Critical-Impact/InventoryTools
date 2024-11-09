using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Widgets;

using Dalamud.Interface.Textures;

public class HoverButton
{
    public HoverButton(Vector2? size = null, int framePadding = 0, Vector2? uv0 = null, Vector2? uv1 = null, Vector4? bgColor = null, Vector4? tintColor = null, Vector4? bgColorHover = null, Vector4? tintColorHover = null)
    {
        _framePadding = framePadding;
        Size = size ?? new Vector2(22,22);
        _uv0 = uv0 ?? new Vector2(0, 0);
        _uv1 = uv1 ?? new Vector2(1, 1);
        _bgColor = bgColor ?? new Vector4(1, 1, 1, 0);
        _tintColor = tintColor ?? new Vector4(1, 1, 1, 1);
        _bgColorHover = bgColorHover ?? _bgColor;
        _tintColorHover = tintColorHover;
        _buttonState = new Dictionary<string, bool>();
    }

    private Dictionary<string, bool> _buttonState;

    public Vector2 Size
    {
        get => _size;
        set => _size = value;
    }

    public bool Draw(ISharedImmediateTexture sharedImmediateTexture, string id, Vector2? size = null)
    {
        return Draw(sharedImmediateTexture.GetWrapOrEmpty().ImGuiHandle, id, size);
    }

    public bool Draw(nint imGuiHandle, string id, Vector2? size = null)
    {
        var isHovered = _buttonState.ContainsKey(id) && _buttonState[id];
        var success = false;
        using var pushId  = ImRaii.PushId(id);
        if (ImGui.ImageButton(imGuiHandle, size ?? Size * ImGui.GetIO().FontGlobalScale, _uv0, _uv1, _framePadding,
                isHovered ? _bgColorHover : _bgColor, isHovered ? _tintColorHover ?? ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered] : _tintColor))
        {
            success = true;
        }

        if (ImGui.IsItemHovered())
        {
            _buttonState[id] = true;
        }
        else
        {
            _buttonState[id] = false;
        }

        return success;
    }
    private Vector2 _size;
    private int _framePadding;
    private readonly Vector2 _uv0;
    private readonly Vector2 _uv1;
    private readonly Vector4 _bgColor;
    private readonly Vector4 _tintColor;
    private readonly Vector4 _bgColorHover;
    private readonly Vector4? _tintColorHover;
}
