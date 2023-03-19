using System;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using OtterGui.Raii;

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
    
    public static void PushClipRectFullScreen() => ImGui.GetWindowDrawList().PushClipRectFullScreen();
    
    public static bool AddHeaderIconButton(IIconService storage, string id, int icon, float zoom, Vector2 offset, float rotation, uint color , Vector2 buttonOffset)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var prevCursorPos = ImGui.GetCursorPos();
        var buttonSize = new Vector2(20 * scale);
        var buttonPos = new Vector2(ImGui.GetWindowWidth() - buttonSize.X - 34 * scale - ImGui.GetStyle().FramePadding.X * 2 + buttonOffset.X, 2 + buttonOffset.Y);
        ImGui.SetCursorPos(buttonPos);
        PushClipRectFullScreen();

        var pressed = false;
        ImGui.InvisibleButton(id, buttonSize);
        var itemMin = ImGui.GetItemRectMin();
        var itemMax = ImGui.GetItemRectMax();
        if (ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(itemMin, itemMax, false))
        {
            var halfSize = ImGui.GetItemRectSize() / 2;
            var center = itemMin + halfSize;
            ImGui.GetWindowDrawList().AddCircleFilled(center, halfSize.X, ImGui.GetColorU32(ImGui.IsMouseDown(ImGuiMouseButton.Left) ? ImGuiCol.ButtonActive : ImGuiCol.ButtonHovered));
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                pressed = true;
        }

        ImGui.SetCursorPos(buttonPos);
        DrawIcon(storage.LoadIcon(icon), new IconSettings
        {
            size = buttonSize,
            zoom = zoom,
            offset = offset,
            rotation = rotation,
            color = color,
        });

        ImGui.PopClipRect();
        ImGui.SetCursorPos(prevCursorPos);

        return pressed;
    }
    
    private static void DrawIcon(ImGuiScene.TextureWrap icon, IconSettings settings) => ImGui.GetWindowDrawList().AddIcon(icon, ImGui.GetItemRectMin(), settings);

    public class IconSettings
    {
        [Flags]
        public enum CooldownStyle
        {
            None = 0,
            Number = 1,
            Disable = 2,
            Cooldown = 4,
            GCDCooldown = 8,
            ChargeCooldown = 16
        }

        public Vector2 size = Vector2.One;
        public float zoom = 1;
        public Vector2 offset = Vector2.Zero;
        public double rotation = 0;
        public bool flipped = false;
        public uint color = 0xFFFFFFFF;
        public bool hovered = false;
        public float activeTime = -1;
        public bool frame = false;
        public float cooldownCurrent = -1;
        public float cooldownMax = -1;
        public uint cooldownAction = 0;
        public CooldownStyle cooldownStyle = CooldownStyle.None;
    }
    
    public static void AddIcon(this ImDrawListPtr drawList,ImGuiScene.TextureWrap tex, Vector2 pos, IconSettings settings)
    {
        if (tex == null) return;

        var z = 0.5f / settings.zoom;
        var uv1 = new Vector2(0.5f - z + settings.offset.X, 0.5f - z + settings.offset.Y);
        var uv3 = new Vector2(0.5f + z + settings.offset.X, 0.5f + z + settings.offset.Y);

        var p1 = pos;
        var p2 = pos + new Vector2(settings.size.X, 0);
        var p3 = pos + settings.size;
        var p4 = pos + new Vector2(0, settings.size.Y);

        var rCos = (float)Math.Cos(settings.rotation);
        var rSin = (float)-Math.Sin(settings.rotation);
        var uvHalfSize = (uv3 - uv1) / 2;
        var uvCenter = uv1 + uvHalfSize;
        uv1 = uvCenter + RotateVector(-uvHalfSize, rCos, rSin);
        var uv2 = uvCenter + RotateVector(new Vector2(uvHalfSize.X, -uvHalfSize.Y), rCos, rSin);
        uv3 = uvCenter + RotateVector(uvHalfSize, rCos, rSin);
        var uv4 = uvCenter + RotateVector(new Vector2(-uvHalfSize.X, uvHalfSize.Y), rCos, rSin);

        if (settings.hovered && !settings.frame)
            drawList.AddRectFilled(p1, p3, (settings.activeTime != 0) ? ImGui.GetColorU32(ImGuiCol.ButtonActive) : ImGui.GetColorU32(ImGuiCol.ButtonHovered));

        if (!settings.flipped)
            drawList.AddImageQuad(tex.ImGuiHandle, p1, p2, p3, p4, uv1, uv2, uv3, uv4, settings.color);
        else
            drawList.AddImageQuad(tex.ImGuiHandle, p2, p1, p4, p3, uv1, uv2, uv3, uv4, settings.color);

        var size = settings.size;
        var halfSize = size / 2;
        var center = pos + halfSize;

        ImGui.GetForegroundDrawList().AddImage(tex.ImGuiHandle, center, center, new System.Numerics.Vector2(0,0), new System.Numerics.Vector2(1,1), ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1 - 0.65f)));
    }
    
    
    public static Vector2 RotateVector(Vector2 v, float a)
    {
        var aCos = (float)Math.Cos(a);
        var aSin = (float)Math.Sin(a);
        return RotateVector(v, aCos, aSin);
    }

    public static Vector2 RotateVector(Vector2 v, float aCos, float aSin) => new(v.X * aCos - v.Y * aSin, v.X * aSin + v.Y * aCos);

}