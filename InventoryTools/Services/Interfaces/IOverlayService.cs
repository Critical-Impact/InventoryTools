using System.Collections.Generic;
using CriticalCommonLib.Services.Ui;
using InventoryTools.GameUi;
using InventoryTools.Logic;

namespace InventoryTools.Services.Interfaces;

public interface IOverlayService
{
    void RefreshOverlayStates();
    FilterState? LastState { get; }
    Dictionary<string, IAtkOverlayState> Overlays { get; }
    void UpdateState(FilterState? filterState);
    void AddOverlay(IAtkOverlayState overlayState);
    void RemoveOverlay(WindowName windowName);
    void RemoveOverlay(IAtkOverlayState overlayState);
    void ClearOverlays();
    void Dispose();
}