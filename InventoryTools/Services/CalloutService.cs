using System.Numerics;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

public class CalloutService : ICalloutService
{
    private readonly InventoryToolsConfiguration _configuration;

    public CalloutService(InventoryToolsConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool HasSeen(NotificationPopup popup) => _configuration.HasSeenNotification(popup);

    public void MarkSeen(NotificationPopup popup)
    {
        _configuration.MarkNotificationSeen(popup);
        _configuration.IsDirty = true;
    }

    public bool DrawCallout(NotificationPopup popup, string title, string body, Vector2 anchorScreenPos)
    {
        if (HasSeen(popup)) return false;

        var fontScale = ImGui.GetIO().FontGlobalScale;
        var wrapWidth = 320f * fontScale;

        if (ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
        {
            ImGui.SetNextWindowFocus();
        }

        ImGui.SetNextWindowPos(anchorScreenPos + new Vector2(0, 28 * fontScale), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.97f);

        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.AlwaysAutoResize;

        var open = true;
        if (ImGui.Begin($"##callout_{(int)popup}", ref open, flags))
        {
            ImGui.TextColored(new Vector4(1f, 0.85f, 0.3f, 1f), title);
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + wrapWidth);
            ImGui.TextUnformatted(body);
            ImGui.PopTextWrapPos();
            ImGui.Spacing();
            if (ImGui.Button("Got it"))
            {
                MarkSeen(popup);
                open = false;
            }
        }
        ImGui.End();

        if (!open)
        {
            MarkSeen(popup);
        }

        return !HasSeen(popup);
    }
}
