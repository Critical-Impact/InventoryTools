using System.Numerics;
using InventoryTools.Logic;

namespace InventoryTools.Services.Interfaces;

public interface ICalloutService
{
    bool HasSeen(NotificationPopup popup);
    void MarkSeen(NotificationPopup popup);

    /// <summary>
    /// Draws an anchored callout popup attached to a UI element.
    /// Call each frame immediately after drawing the UI element this attaches to.
    /// </summary>
    /// <param name="popup">The popup identifier used to track whether it has been dismissed.</param>
    /// <param name="title">Title text shown in the popup header.</param>
    /// <param name="body">Body text explaining the feature.</param>
    /// <param name="anchorScreenPos">Screen position to anchor the popup to (e.g. ImGui.GetItemRectMin()).</param>
    /// <param name="size">Size of the popup window.</param>
    /// <returns>True while the popup is visible.</returns>
    /// <summary>
    /// Draws an anchored callout popup attached to a UI element.
    /// Call each frame immediately after drawing the UI element this attaches to.
    /// The popup auto-sizes to fit its content.
    /// </summary>
    /// <param name="popup">The popup identifier used to track whether it has been dismissed.</param>
    /// <param name="title">Title text shown in the popup header.</param>
    /// <param name="body">Body text explaining the feature.</param>
    /// <param name="anchorScreenPos">Screen position to anchor the popup to (e.g. ImGui.GetItemRectMin()).</param>
    /// <returns>True while the popup is visible.</returns>
    bool DrawCallout(NotificationPopup popup, string title, string body, Vector2 anchorScreenPos);
}
