using System.Runtime.CompilerServices;
using ImGuiNET;

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

    }
}