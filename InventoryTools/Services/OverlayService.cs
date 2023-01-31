using System;
using System.Collections.Generic;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game;
using Dalamud.Logging;
using InventoryTools.GameUi;
using InventoryTools.Logic;

namespace InventoryTools.Services
{
    public class OverlayService : IDisposable
    {
        private FilterService _filterService;
        private GameUiManager _gameUiManager;
        
        public OverlayService(FilterService filterService, GameUiManager gameUiManager)
        {
            _gameUiManager = gameUiManager;
            PluginService.GameUi.UiVisibilityChanged += GameUiOnUiVisibilityChanged;
            PluginService.GameUi.UiUpdated += GameUiOnUiUpdated;
            _filterService = filterService;
            filterService.FilterModified += FilterServiceOnFilterModified;
            filterService.UiFilterToggled += FilterServiceOnFilterToggled;
            filterService.BackgroundFilterToggled += FilterServiceOnFilterToggled;
            filterService.FilterTableRefreshed += FilterServiceOnFilterTableRefreshed;
            AddOverlay(new RetainerListOverlay());
            AddOverlay(new InventoryExpansionOverlay());
            AddOverlay(new ArmouryBoardOverlay());
            AddOverlay(new InventoryLargeOverlay());
            AddOverlay(new InventoryGridOverlay());
            AddOverlay(new InventoryRetainerLargeOverlay());
            AddOverlay(new InventoryRetainerOverlay());
            AddOverlay(new InventoryBuddyOverlay());
            AddOverlay(new InventoryBuddyOverlay2());
            AddOverlay(new FreeCompanyChestOverlay());
            AddOverlay(new InventoryMiragePrismBoxOverlay());
            AddOverlay(new CabinetWithdrawOverlay());
            Service.Framework.Update += FrameworkOnUpdate;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            RefreshOverlayStates();
        }

        private void FilterServiceOnFilterTableRefreshed(RenderTableBase table)
        {
            if (PluginService.PluginLoaded)
            {
                RefreshOverlayStates();
            }
        }

        private void FilterServiceOnFilterToggled(FilterConfiguration configuration, bool newstate)
        {
            if (PluginService.PluginLoaded)
            {
                RefreshOverlayStates();
            }
        }

        private void FilterServiceOnFilterModified(FilterConfiguration configuration)
        {
            if (PluginService.PluginLoaded)
            {
                RefreshOverlayStates();
            }
        }

        public void RefreshOverlayStates()
        {
            var activeFilter = _filterService.GetActiveUiFilter(false);
            var activeBackgroundFilter = _filterService.GetActiveBackgroundFilter();
            PluginLog.Debug("Overlays refreshing, active filter is " + (activeFilter?.Name ?? "no filter"));
            if (activeFilter != null && _filterService.HasFilterTable(activeFilter))
            {
                UpdateState(new FilterState(){FilterConfiguration = activeFilter, FilterTable = _filterService.GetFilterTable(activeFilter)});
            }
            else if (activeBackgroundFilter != null)
            {
                UpdateState(new FilterState(){FilterConfiguration = activeBackgroundFilter});
            }
            else
            {
                UpdateState(null);
            }
        }

#pragma warning disable CS8618
        public OverlayService(bool test)
#pragma warning restore CS8618
        {
            
        }

        private void FrameworkOnUpdate(Framework framework)
        {
            foreach (var overlay in _overlays)
            {
                if (overlay.Value.NeedsStateRefresh)
                {
                    overlay.Value.UpdateState(_lastState);
                    overlay.Value.NeedsStateRefresh = false;
                }

                if (overlay.Value.HasAddon)
                {
                    overlay.Value.Update();
                }
            }
        }

        private Dictionary<WindowName, IAtkOverlayState> _overlays = new();
        private HashSet<WindowName> _setupHooks = new();
        private Dictionary<WindowName, DateTime> _lastUpdate = new();
        private FilterState? _lastState;

        public FilterState? LastState => _lastState;

        public Dictionary<WindowName, IAtkOverlayState> Overlays
        {
            get => _overlays;
        }

        public void UpdateState(FilterState? filterState)
        {
            foreach (var overlay in _overlays)
            {
                overlay.Value.UpdateState(filterState);
                _lastState = filterState;
            }
        }

        public void SetupUpdateHook(IAtkOverlayState overlayState)
        {
            if (_setupHooks.Contains(overlayState.WindowName))
            {
                return;
            }
            var result = PluginService.GameUi.WatchWindowState(overlayState.WindowName);
            if (result)
            {
                _setupHooks.Add(overlayState.WindowName);
            }
        }

        public void AddOverlay(IAtkOverlayState overlayState)
        {
            if (!Overlays.ContainsKey(overlayState.WindowName))
            {
                Overlays.Add(overlayState.WindowName, overlayState);
                overlayState.Setup();
                overlayState.Draw();
            }
            else
            {
                PluginLog.Error("Attempted to add an overlay that is already registered.");
            }
        }

        public void RemoveOverlay(WindowName windowName)
        {
            if (Overlays.ContainsKey(windowName))
            {
                Overlays[windowName].Clear();
                Overlays.Remove(windowName);
            }
        }

        public void RemoveOverlay(IAtkOverlayState overlayState)
        {
            if (Overlays.ContainsKey(overlayState.WindowName))
            {
                Overlays.Remove(overlayState.WindowName);
                overlayState.Clear();
            }
        }

        public void ClearOverlays()
        {
            foreach (var overlay in _overlays)
            {
                RemoveOverlay(overlay.Value);
            }
        }
        private void GameUiOnUiVisibilityChanged(WindowName windowname, bool? windowstate)
        {
            if (PluginService.PluginLoaded)
            {
                if (windowstate == true)
                {
                    RefreshOverlayStates();
                }

                if (_overlays.ContainsKey(windowname))
                {
                    var overlay = _overlays[windowname];
                    if (windowstate.HasValue && windowstate.Value)
                    {
                        SetupUpdateHook(overlay);
                        if (_lastState != null && !overlay.HasState)
                        {
                            overlay.UpdateState(_lastState);
                        }
                    }

                    if (windowstate.HasValue && !windowstate.Value)
                    {
                        overlay.UpdateState(null);
                    }

                    overlay.Draw();
                }
            }
        }
        
        private void GameUiOnUiUpdated(WindowName windowname)
        {
            if (PluginService.PluginLoaded)
            {
                if (_overlays.ContainsKey(windowname))
                {
                    var overlay = _overlays[windowname];
                    if (!_lastUpdate.ContainsKey(windowname))
                    {
                        _lastUpdate[windowname] = DateTime.Now.AddMilliseconds(50);
                        if (_lastState != null && !overlay.HasState)
                        {
                            overlay.UpdateState(_lastState);
                        }
                        else
                        {
                            overlay.Draw();
                        }
                    }
                    else if (_lastUpdate[windowname] <= DateTime.Now)
                    {
                        if (_lastState != null && !overlay.HasState)
                        {
                            overlay.UpdateState(_lastState);
                        }
                        else
                        {
                            overlay.Draw();
                        }

                        _lastUpdate[windowname] = DateTime.Now.AddMilliseconds(50);
                    }
                }
            }
        }
        
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                Service.Framework.Update -= FrameworkOnUpdate;
                ClearOverlays();
                _filterService.FilterModified -= FilterServiceOnFilterModified;
                _filterService.UiFilterToggled -= FilterServiceOnFilterToggled;
                _filterService.BackgroundFilterToggled -= FilterServiceOnFilterToggled;
                PluginService.GameUi.UiVisibilityChanged -= GameUiOnUiVisibilityChanged;
                PluginService.GameUi.UiUpdated -= GameUiOnUiUpdated;
                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
            }
            _disposed = true;         
        }
    }
}