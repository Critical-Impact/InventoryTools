using System;
using System.Globalization;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools.Ui.Widgets;

public class HorizontalSplitter
{
    private float _height;
    private float? _splitterResizeBuffer;
    public readonly Vector2 MaxRange;
    private float _bottomHeight;
    private bool _allowCollapse;

    public float Height
    {
        get => _height;
        set
        {
            _height = value;
        }
    }

    public bool CollapsedTop
    {
        get => _collapsedTop;
        set
        {
            if (value && _collapsedBottom)
            {
                _collapsedTop = value;
                _collapsedBottom = false;
            }
            else
            {
                _collapsedTop = value;
            }
        }
    }

    public bool CollapsedBottom
    {
        get => _collapsedBottom;
        set
        {
            if (value && _collapsedTop)
            {
                _collapsedBottom = value;
                _collapsedTop = false;
            }
            else
            {
                _collapsedBottom = value;
            }
        }
    }

    public HorizontalSplitter(float height, Vector2 maxRange, bool allowCollapse = false)
    {
        MaxRange = maxRange;
        _height = height;
        _allowCollapse = allowCollapse;
    }

    public float DraggerSize { get; set; } = 4;

    private float _currentOffset = 0;
    private float? _previousFrameHeight = null;
    private bool _collapsedTop = false;
    private bool _collapsedBottom = false;

    public float? Draw(Action<bool> drawTop, Action<bool> drawBottom, string? headerTextTop = null, string? headerTextBottom = null)
    {
        var frameHeight = ImGui.GetWindowHeight();

        if (CollapsedTop && !CollapsedBottom)
        {
            using (var top = ImRaii.Child("Top##Top"))
            {
                if (top.Success)
                {
                    ImGui.SetNextItemOpen(!CollapsedTop);
                    if (ImGui.CollapsingHeader(headerTextTop + "##headerTextTop",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        CollapsedTop = false;
                        drawTop.Invoke(true);
                    }
                    else
                    {
                        CollapsedTop = true;
                        drawTop.Invoke(false);
                    }

                    ImGui.SetNextItemOpen(!CollapsedBottom);
                    if (ImGui.CollapsingHeader(headerTextBottom + "##headerTextBottom",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        CollapsedBottom = false;
                        drawBottom.Invoke(true);
                    }
                    else
                    {
                        CollapsedBottom = true;
                        drawBottom.Invoke(false);
                    }
                }
            }
            return null;
        }
        else if (CollapsedBottom && !CollapsedTop)
        {
            using (var bottom = ImRaii.Child("Bottom##Bottom"))
            {
                if (bottom.Success)
                {
                    CollapsedTop = false;
                    ImGui.SetNextItemOpen(!CollapsedTop);
                    if (ImGui.CollapsingHeader(headerTextTop + "##headerTextTop",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        var framePadding = ImGui.CalcTextSize(headerTextTop).Y + (ImGui.GetStyle().FramePadding.Y * 2) +
                                           (ImGui.GetStyle().CellPadding.Y * 2) * ImGui.GetIO().FontGlobalScale;
                        using (ImRaii.Child("##HeaderSpacer", new Vector2(0, -framePadding)))
                        {
                            drawTop.Invoke(true);
                        }
                    }
                    else
                    {
                        CollapsedTop = true;
                        drawTop.Invoke(false);
                    }

                    ImGui.SetNextItemOpen(!CollapsedBottom);
                    if (ImGui.CollapsingHeader(headerTextBottom + "##headerTextBottom",
                            ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        CollapsedBottom = false;
                        drawBottom.Invoke(true);
                    }
                    else
                    {
                        CollapsedBottom = true;
                        drawBottom.Invoke(false);
                    }
                }
            }
            return null;
        }

        float? returnValue = null;
        var currentHeight = Math.Clamp(Height + _currentOffset, MaxRange.X, Math.Max(MaxRange.Y, frameHeight - MaxRange.Y)) ;
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            if (_splitterResizeBuffer != null)
            {
                _splitterResizeBuffer = null;
                Height = currentHeight;
                _currentOffset = 0;
                returnValue = Height;
            }
        }

        if (_previousFrameHeight == null)
        {
            _previousFrameHeight = frameHeight;
        }
        if (Math.Abs(_previousFrameHeight.Value - frameHeight) > 0.01)
        {
            var difference = _previousFrameHeight.Value - frameHeight;
            Height -= difference;
            _previousFrameHeight = frameHeight;

        }

        using (var topChild = ImRaii.Child("Top##Top", new Vector2(-1.0f, currentHeight) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (topChild.Success)
            {
                if (_allowCollapse && headerTextTop != null)
                {
                    if (!CollapsedTop)
                    {
                        ImGui.SetNextItemOpen(true);
                    }
                    if(ImGui.CollapsingHeader(headerTextTop + "##headerTextTop", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        CollapsedTop = false;
                        drawTop.Invoke(true);
                    }
                    else
                    {
                        CollapsedTop = true;
                        drawTop.Invoke(false);
                    }
                }
                else
                {
                    drawTop.Invoke(true);
                }
            }
        }

        var availableWidth = ImGui.GetContentRegionAvail().X;
        var splitterSize = availableWidth * 0.80f;
        var splitterOffset = availableWidth * 0.10f;
        using (ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, 100.0f))
        {
            using (ImRaii.PushColor(ImGuiCol.Button, ImGuiColors.DalamudGrey))
            {
                ImGui.SetCursorPosX(splitterOffset + ImGui.GetCursorPosX());
                using (var dragger = ImRaii.Child("Dragger", new System.Numerics.Vector2(splitterSize, DraggerSize),
                           false))
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
                                _currentOffset = mouseDragDelta.Y;
                            }
                        }
                    }
                }
            }
        }

        using (var bottomChild = ImRaii.Child("Bottom##Bottom", new Vector2(-1, -1) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (bottomChild.Success)
            {
                _bottomHeight = ImGui.GetWindowHeight();
                if (_allowCollapse && headerTextBottom != null)
                {
                    if (!CollapsedBottom)
                    {
                        ImGui.SetNextItemOpen(true);
                    }
                    if(ImGui.CollapsingHeader(headerTextBottom + "##headerTextBottom", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                    {
                        CollapsedBottom = false;
                        drawBottom.Invoke(true);
                    }
                    else
                    {
                        CollapsedBottom = true;
                        drawBottom.Invoke(false);
                    }
                }
                else
                {
                    drawBottom.Invoke(true);
                }
            }
            else
            {
                Height -= 1;
            }
        }

        return returnValue;
    }
}