using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays;

public interface IGameOverlay : IAtkOverlayState, IDisposable
{
    bool ShouldDraw { get; set; }
    public WindowName WindowName { get; }
    public HashSet<WindowName>? ExtraWindows { get; }
    
    public bool HasAddon { get; }
    
    public bool Enabled { get; set; }

    bool Draw();
    void Setup();
    void Update();
}

public abstract class GameOverlay<T> : IGameOverlay where T : AtkOverlay
{
    public GameOverlay(ILogger logger, T overlay)
    {
        Logger = logger;
        AtkOverlay = overlay;
        AtkOverlay.AtkUpdated += AtkOverlayOnAtkUpdated;
    }

    private void AtkOverlayOnAtkUpdated()
    {
        Logger.LogTrace("ATK overlay event received, requesting state refresh.");
        NeedsStateRefresh = true;
    }

    public ILogger Logger { get; }
    public T AtkOverlay { get; }
    
    public abstract bool ShouldDraw { get; set; }

    public virtual WindowName WindowName => AtkOverlay.WindowName;
    public virtual HashSet<WindowName>? ExtraWindows => AtkOverlay.ExtraWindows;
    public virtual bool HasAddon => AtkOverlay.HasAddon;

    public abstract bool Draw();
    public abstract void Setup();
        
    public virtual void Update()
    {
        AtkOverlay.Update();
    }

    public bool Enabled { get; set; } = true;
    public abstract bool HasState { get; set; }
    public abstract bool NeedsStateRefresh { get; set; }
    public abstract void UpdateState(FilterState? newState);
    public abstract void Clear();

    public void Dispose()
    {
        AtkOverlay.AtkUpdated -= AtkOverlayOnAtkUpdated;
    }
}