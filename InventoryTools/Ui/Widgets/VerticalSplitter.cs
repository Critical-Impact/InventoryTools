using System;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Ui.Widgets;

public class VerticalSplitter
{
    private float _width;
    private float? _splitterResizeBuffer;
    private readonly Vector2 _maxRange;

    public float Width
    {
        get => _width;
        set
        {
            _width = Math.Clamp(value, _maxRange.X, _maxRange.Y);
        }
    }

    public VerticalSplitter(float width, Vector2 maxRange)
    {
        _maxRange = maxRange;
        _width = width;
    }

    public float DraggerSize { get; set; } = 2;
    

    public void Draw(Action drawLeft, Action drawRight)
    {
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_splitterResizeBuffer != null)
            {
                _splitterResizeBuffer = null;
            }
        }
        
        using (var leftChild = ImRaii.Child("Left", new Vector2(Width, -1.0f) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (leftChild.Success)
            {
                ImGui.TextUnformatted("Width:" + Width.ToString());
                ImGui.TextUnformatted("SplitterResizeBuffer:" + (_splitterResizeBuffer?.ToString() ?? "null"));
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    ImGui.TextUnformatted("Mouse is down");
                }

                drawLeft.Invoke();
            }
        }
        ImGui.SameLine();
        using (var dragger = ImRaii.Child("Dragger", new System.Numerics.Vector2(DraggerSize, 0), false))
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
                            _splitterResizeBuffer = Width;
                        }
                        var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                        PluginLog.Log(mouseDragDelta.X.ToString());
                        Width = _splitterResizeBuffer.Value + mouseDragDelta.X;
                    }
                }
            }
        }
        ImGui.SameLine();
        using (var rightChild = ImRaii.Child("Right", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (rightChild.Success)
            {
                drawRight.Invoke();
            }
        }
    }
}