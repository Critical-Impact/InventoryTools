using System;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Widgets;

public static class ImGuiUtil
{

    public static bool OpenNameField(string popupName, ref string newName)
    {
        using var popup = ImRaii.Popup(popupName);
        if (!popup)
            return false;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            ImGui.CloseCurrentPopup();

        ImGui.SetNextItemWidth(300 * ImGuiHelpers.GlobalScale);
        var enterPressed = ImGui.InputTextWithHint("##newName", "Enter New Name...", ref newName, 64, ImGuiInputTextFlags.EnterReturnsTrue);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere(-1);

        if (!enterPressed)
            return false;

        ImGui.CloseCurrentPopup();
        return true;
    }
    
    public static void VerticalAlignText( string text, int cellHeight, bool autoWrap, float? xOffset = null)
    {
        var columnWidth = ImGui.GetColumnWidth();
        var frameHeight = cellHeight / 2.0f;
        var calcText = ImGui.CalcTextSize(text);
        var textHeight = calcText.X >= columnWidth ? 0 : calcText.Y / 2.0f;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
        if (xOffset != null)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset.Value);
        }
        if (autoWrap)
        {
            ImGui.PushTextWrapPos();
        }
        ImGui.TextUnformatted(text);
        if (autoWrap)
        {
            ImGui.PopTextWrapPos();
        }
    }

    public static void VerticalAlignTextDisabled( string text, int cellHeight, bool autoWrap, float? xOffset = null)
    {
        var columnWidth = ImGui.GetColumnWidth();
        var frameHeight = cellHeight / 2.0f;
        var calcText = ImGui.CalcTextSize(text);
        var textHeight = calcText.X >= columnWidth ? 0 : calcText.Y / 2.0f;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
        if (xOffset != null)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + xOffset.Value);
        }
        if (autoWrap)
        {
            ImGui.PushTextWrapPos();
        }
        ImGui.TextDisabled(text);
        if (autoWrap)
        {
            ImGui.PopTextWrapPos();
        }
    }

    public static void VerticalAlignTextColored(string text, Vector4 colour, int cellHeight,  bool autoWrap)
    {
        var columnWidth = ImGui.GetColumnWidth();
        var frameHeight = cellHeight / 2.0f;
        var calcText = ImGui.CalcTextSize(text);
        if (calcText.X <= columnWidth || !autoWrap)
        {
            var textHeight = calcText.Y / 2.0f;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
        }

        if (autoWrap)
        {
            ImGui.PushTextWrapPos();
        }
        using var _ = ImRaii.PushColor(ImGuiCol.Text,colour);
        ImGui.TextUnformatted(text);
        if (autoWrap)
        {
            ImGui.PopTextWrapPos();
        }
    }


    public static void VerticalAlignButton(int cellHeight)
    {
        var frameHeight = cellHeight / 2.0f;
        var textHeight = (ImGui.GetFontSize() * 1 + ImGui.GetStyle().FramePadding.Y * 2) / 2.0f;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
    }
    
    public static bool? ConfirmPopup(string label, Vector2 size, Action content)
    {
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Always, new Vector2(0.5f));
        ImGui.SetNextWindowSize(size);
        using var pop = ImRaii.Popup(label, ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
        if (pop)
        {
            content();
            const string yesButtonText   = "Yes";
            const string noButtonText   = "No";
            var          yesButtonSize   = Math.Max(size.X / 5, ImGui.CalcTextSize(yesButtonText).X + 2 * ImGui.GetStyle().FramePadding.X);
            var          noButtonSize   = Math.Max(size.X / 5, ImGui.CalcTextSize(yesButtonText).X + 2 * ImGui.GetStyle().FramePadding.X);
            ImGui.SetCursorPos(new Vector2(2 * ImGui.GetStyle().FramePadding.X, size.Y - ImGui.GetFrameHeight() * 1.75f));
            if (ImGui.Button(yesButtonText, new Vector2(yesButtonSize, 0)))
            {
                ImGui.CloseCurrentPopup();
                return true;
            }
            ImGui.SetCursorPos(new Vector2(yesButtonSize + 20, size.Y - ImGui.GetFrameHeight() * 1.75f));

            if (ImGui.Button(noButtonText, new Vector2(noButtonSize, 0)))
            {
                ImGui.CloseCurrentPopup();
                return false;
            }
        }

        return null;
    }
}