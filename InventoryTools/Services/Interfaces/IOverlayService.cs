using System.Collections.Generic;
using InventoryTools.Logic;
using InventoryTools.Overlays;
using Microsoft.Extensions.Hosting;

namespace InventoryTools.Services.Interfaces;

public interface IOverlayService : IHostedService
{
    void RefreshOverlayStates();
    FilterState? LastState { get; }
    List<IGameOverlay> Overlays { get; }
    void UpdateState(FilterState? filterState);
    void EnableOverlay(IGameOverlay overlayState);
    void DisableOverlay(IGameOverlay overlayState);
    void ClearOverlays();
}