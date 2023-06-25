using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;
using OtterGui.Raii;

namespace InventoryTools.Ui.Widgets;

public class HoverButton
{
    public HoverButton(TextureWrap textureWrap, Vector2 size, int framePadding = 0, Vector2? uv0 = null, Vector2? uv1 = null, Vector4? bgColor = null, Vector4? tintColor = null, Vector4? bgColorHover = null, Vector4? tintColorHover = null)
    {
        _framePadding = framePadding;
        Size = size;
        _textureWrap = textureWrap;
        _uv0 = uv0 ?? new Vector2(0, 0);
        _uv1 = uv1 ?? new Vector2(1, 1);
        _bgColor = bgColor ?? new Vector4(1, 1, 1, 0);
        _tintColor = tintColor ?? new Vector4(1, 1, 1, 1);
        _bgColorHover = bgColorHover ?? _bgColor;
        _tintColorHover = tintColorHover ?? ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered];
        _buttonState = new Dictionary<string, bool>();
    }

    private Dictionary<string, bool> _buttonState;

    public Vector2 Size
    {
        get => _size;
        set => _size = value;
    }

    public bool Draw(string id, Vector2? size = null)
    {
        var isHovered = _buttonState.ContainsKey(id) && _buttonState[id];
        var success = false;
        using var pushId  = ImRaii.PushId(id);
        if (ImGui.ImageButton(_textureWrap.ImGuiHandle, size ?? Size * ImGui.GetIO().FontGlobalScale, _uv0, _uv1, _framePadding,
                isHovered ? _bgColorHover : _bgColor, isHovered ? _tintColorHover : _tintColor))
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
    private string _id;
    private TextureWrap _textureWrap;
    private Vector2 _size;
    private int _framePadding;
    private readonly Vector2 _uv0;
    private readonly Vector2 _uv1;
    private readonly Vector4 _bgColor;
    private readonly Vector4 _tintColor;
    private readonly Vector4 _bgColorHover;
    private readonly Vector4 _tintColorHover;
}
