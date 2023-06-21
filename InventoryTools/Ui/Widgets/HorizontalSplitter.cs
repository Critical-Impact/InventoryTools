using System;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Ui.Widgets;

public class HorizontalSplitter
{
    private float _height;
    private float? _splitterResizeBuffer;
    private readonly Vector2 _maxRange;
    private float _bottomHeight;

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
        }
    }

    public HorizontalSplitter(float height, Vector2 maxRange)
    {
        _maxRange = maxRange;
        _height = height;
    }

    public float DraggerSize { get; set; } = 2;
    

    public void Draw(Action drawLeft, Action drawRight)
    {
        var frameHeight = ImGui.GetWindowHeight();
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_splitterResizeBuffer != null)
            {
                _splitterResizeBuffer = null;
            }
        }
        
        
        if (_bottomHeight < _maxRange.Y)
        {
            Height -= (_maxRange.Y - _bottomHeight);
            _bottomHeight = _maxRange.Y;
        }

        if (Height < 0)
        {
            Height = _maxRange.X;
        }

        if (Height < _maxRange.X)
        {
            Height = _maxRange.X;
        }

        if (_bottomHeight < _maxRange.Y)
        {
            Height--;
        }
        
        using (var topChild = ImRaii.Child("Top", new Vector2(-1.0f, Height) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (topChild.Success)
            {
                drawLeft.Invoke();
            }
        }
        using (var dragger = ImRaii.Child("Dragger", new System.Numerics.Vector2(0, DraggerSize), false))
        {
            if (dragger.Success)
            {
                ImGui.Button("DraggerBtn", new(-1, -1));
                if (ImGui.IsItemActive())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        if (_splitterResizeBuffer == null)
                        {
                            _splitterResizeBuffer = Height;
                        }
                        var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                        var mouseDeltaOffset = Math.Max(0,mouseDragDelta.Y - _splitterResizeBuffer.Value);
                        PluginLog.Log(mouseDragDelta.Y.ToString());
                        PluginLog.Log("Offset:" + mouseDeltaOffset.ToString());
                        PluginLog.Log("Bottom Height:" + _bottomHeight.ToString());
                        var newHeight = _splitterResizeBuffer.Value + mouseDragDelta.Y;
                        newHeight = Math.Clamp(newHeight, _maxRange.X, frameHeight);

                        if (_bottomHeight > 100 || mouseDeltaOffset < 0)
                        {
                            Height = newHeight;
                        }
                        else if(mouseDragDelta.Y < 0)
                        {
                            Height = _splitterResizeBuffer.Value + mouseDragDelta.Y;
                            var a = "";
                        }
                    }
                }
            }
        }
        using (var bottomChild = ImRaii.Child("Bottom", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (bottomChild.Success)
            {
                _bottomHeight = ImGui.GetWindowHeight();
                drawRight.Invoke();
            }
            else
            {
                Height -= 1;
            }
        }
        
        
        if (_bottomHeight < _maxRange.Y)
        {
            Height -= (_maxRange.Y - _bottomHeight);
            _bottomHeight = _maxRange.Y;
        }
    }
}