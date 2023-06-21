using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Ui.Widgets;

public class HorizontalMultiSplitter
{
    private Dictionary<int,float> _splitterResizeBuffer;
    private float[] _heights;
    private Vector2[] _maxSizes;
    private float? _windowHeight;

    public HorizontalMultiSplitter(float[] heights, Vector2[] maxSizes)
    {
        _splitterResizeBuffer = new Dictionary<int, float>();
        _maxSizes = maxSizes;
        _heights = heights;
    }

    public float DraggerSize { get; set; } = 2;

    public float? WindowHeight
    {
        get
        {
            return _windowHeight;
        }
        set
        {
            var newWindowHeight = value  - _heights[^1];
            if (newWindowHeight != null)
            {
                if (_windowHeight != null)
                {
                    var oldWindowHeight = _windowHeight;
                    if (_windowHeight != newWindowHeight)
                    {
                        _windowHeight = newWindowHeight;
                        for (int i = 0; i < _heights.Length - 1; i++)
                        {
                            _heights[i] = _heights[i] / oldWindowHeight.Value * newWindowHeight.Value;
                        }
                    }
                }
                else
                {
                    _windowHeight = newWindowHeight;
                }
            }
        }
    }
    

    public void Draw(params Action[] draw)
    {
        WindowHeight = ImGui.GetWindowHeight();
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _splitterResizeBuffer.Clear();
        }
        
        for (var index = 0; index < _heights.Length; index++)
        {
            var _height = _heights[index];

            if (index != 0)
            {
                using (var dragger =
                       ImRaii.Child("Dragger" + index, new System.Numerics.Vector2(0, DraggerSize), false))
                {
                    if (dragger.Success)
                    {
                        ImGui.Button("DraggerBtn" + index, new(-1, -1));
                        if (ImGui.IsItemActive())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                            {
                                for (var index2 = 0; index2 < _heights.Length; index2++)
                                {
                                    if (!_splitterResizeBuffer.ContainsKey(index2))
                                    {
                                        _splitterResizeBuffer[index2] = _heights[index2];
                                    }
                                }
                                
                                if (index == _heights.Length - 1)
                                {
                                    var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                    if (mouseDragDelta.Y >= 200)
                                    {
                                        
                                    }
                                    PluginLog.Log(mouseDragDelta.Y.ToString());
                                    var newHeight = Math.Clamp(_splitterResizeBuffer[index] - mouseDragDelta.Y, _maxSizes[index].X, _maxSizes[index].Y);
                                    var remainingHeight = _splitterResizeBuffer.SkipLast(1).Sum(c => c.Value) + newHeight;
                                    var totalHeight = _splitterResizeBuffer.Sum(c => c.Value);
                                    
                                    for (var index2 = 0; index2 < _heights.Length - 1; index2++)
                                    {
                                        var currentHeight = _splitterResizeBuffer[index2];
                                        var newHeightFill = currentHeight / remainingHeight * totalHeight;
                                        _heights[index2] = newHeightFill;
                                    }
                                    
                                    _heights[index] = newHeight;
                                }
                                else if (index == 1)
                                {
                                    var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                    var newHeight = Math.Clamp(_splitterResizeBuffer[index - 1] + mouseDragDelta.Y, _maxSizes[index - 1].X, _maxSizes[index - 1].Y);
                                    
                                    //The total height without the first item
                                    var currentTotalHeight = _splitterResizeBuffer.Sum(c => c.Value) - _splitterResizeBuffer[index - 1];
                                    //Total height
                                    var totalHeight = _splitterResizeBuffer.Sum(c => c.Value);
                                    //The total height without the new height of the first item
                                    var newTotalHeight = totalHeight - newHeight;
                                    
                                    for (var index2 = index; index2 < _heights.Length - 1; index2++)
                                    {
                                        var currentHeight = _splitterResizeBuffer[index2];
                                        var newHeightFill = currentHeight / currentTotalHeight * newTotalHeight;
                                        _heights[index2] = newHeightFill;
                                    }
                                    
                                    //Clamp values if required
                                    for (var index2 = _heights.Length - 1; index2 >= 1; index2--)
                                    {
                                        var sizeLimit = _maxSizes[index2];
                                        if (sizeLimit.X > _heights[index2])
                                        {
                                            var excess = _heights[index2] - sizeLimit.X;
                                            _heights[index2] = sizeLimit.X;
                                            _heights[index2 - 1] += excess;
                                        }
                                    }
                                    
                                    
                                    
                                    _heights[index - 1] = newHeight;
                                }
                                else
                                {
                                    var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                    var newHeight = Math.Clamp(_splitterResizeBuffer[index] - mouseDragDelta.Y, _maxSizes[index].X, _maxSizes[index].Y);
                                    var heightAfter = _splitterResizeBuffer.Skip(index + 1).Sum(c => c.Value);
                                    var remainingHeight = _splitterResizeBuffer.Sum(c => c.Value) + newHeight - heightAfter;
                                    var totalHeight = _splitterResizeBuffer.Sum(c => c.Value);
                                    
                                    for (var index2 = 0; index2 < _heights.Length - index; index2++)
                                    {
                                        var currentHeight = _splitterResizeBuffer[index2];
                                        var newHeightFill = currentHeight / remainingHeight * totalHeight;
                                        _heights[index2] = newHeightFill;
                                    }
                                    
                                    _heights[index] = newHeight;
                                    
                                    // var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                    // PluginLog.Log(mouseDragDelta.Y.ToString());
                                    // var newHeight = _splitterResizeBuffer[index] + mouseDragDelta.Y;
                                    // newHeight = Math.Clamp(newHeight, _maxSizes[index].X,
                                    //     frameHeight - _maxSizes[index].Y);
                                    // _heights[index - 1] = newHeight;
                                }
                            }
                        }
                    }
                }
            }

            using (var child = ImRaii.Child("Split" + index, new Vector2(-1.0f, index == (_heights.Length - 1) ? -1 : _heights[index]) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (child.Success)
                {
                    if (index != _heights.Length - 1 || _splitterResizeBuffer.Count == 0)
                    {
                        _heights[index] = ImGui.GetWindowHeight();
                    }
                    ImGui.Text(_heights[index].ToString());
                    //draw[index].Invoke();
                }
            }
        }
    }
}