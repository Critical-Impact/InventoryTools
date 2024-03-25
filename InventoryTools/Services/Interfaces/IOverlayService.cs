using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Ui;
using InventoryTools.GameUi;
using InventoryTools.Logic;
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