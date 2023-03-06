using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using OtterGui.Raii;

namespace InventoryTools
{
    public static class UiHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void HelpMarker(string helpText)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(helpText);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void VerticalCenter(string text)
        {
            var offset = (ImGui.GetWindowSize().Y - ImGui.CalcTextSize(text).Y) / 2.0f;
            ImGui.SetCursorPosY(offset);
            ImGui.TextUnformatted(text);
        }

        public static void CenterElement(float height)
        {
            ImGui.SetCursorPosY((ImGui.GetWindowSize().Y - height) / 2.0f);
        }

        public static void WrapTableColumnElements<T>(string windowId, IEnumerable<T> items, float rowSize, Func<T, bool> drawElement)
        {
            using (ImRaii.Child(windowId, new Vector2(ImGui.GetContentRegionAvail().X, rowSize + ImGui.GetStyle().CellPadding.Y + ImGui.GetStyle().ItemSpacing.Y) * ImGui.GetIO().FontGlobalScale, false))
            {
                var columnWidth = ImGui.GetContentRegionAvail().X * ImGui.GetIO().FontGlobalScale;
                var itemWidth = (rowSize + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                maxItems = maxItems == 0 ? 1 : maxItems;
                var enumerable = items.ToList();
                var count = 1;
                for (var index = 0; index < enumerable.Count; index++)
                {
                    using (ImRaii.PushId(index))
                    {
                        if (drawElement.Invoke(enumerable[index]))
                        {
                            
                            if (count % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }
                            count++;
                        }
                    }
                }
            }
        }

    }
}