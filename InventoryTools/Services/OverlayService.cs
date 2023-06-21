using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Logging;
using InventoryTools.GameUi;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services
{
    public class OverlayService : IDisposable, IOverlayService
    {
        private IFilterService _filterService;
        private IGameUiManager _gameUiManager;
        private IFrameworkService _frameworkService;
        
        public OverlayService(IFilterService filterService, IGameUiManager gameUiManager, IFrameworkService frameworkService)
        {
            _gameUiManager = gameUiManager;
            _gameUiManager.UiVisibilityChanged += GameUiOnUiVisibilityChanged;
            _gameUiManager.UiUpdated += GameUiOnUiUpdated;
            _filterService = filterService;
            _frameworkService = frameworkService;
            filterService.FilterModified += FilterServiceOnFilterModified;
            filterService.UiFilterToggled += FilterServiceOnFilterToggled;
            filterService.BackgroundFilterToggled += FilterServiceOnFilterToggled;
            filterService.FilterRecalculated += FilterServiceOnFilterRecalculated;
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
            AddOverlay(new SelectIconStringOverlay());
            frameworkService.Update += FrameworkOnUpdate;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }

        private void FilterServiceOnFilterRecalculated(FilterConfiguration configuration)
        {
            if (PluginService.PluginLoaded)
            {
                RefreshOverlayStates();
            }
        }        
        
        private void FilterServiceOnFilterTableRefreshed(RenderTableBase tableBase)
        {
            if (PluginService.PluginLoaded)
            {
                RefreshOverlayStates();
            }
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            RefreshOverlayStates();
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

        private void FrameworkOnUpdate(IFrameworkService framework)
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
                if (overlayState.ExtraWindows != null)
                {
                    foreach (var extraWindow in overlayState.ExtraWindows)
                    {
                        _windowOverlayMap[extraWindow] = overlayState.WindowName;
                    }
                }

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
                var atkOverlayState = Overlays[windowName];

                if (atkOverlayState.ExtraWindows != null)
                {
                    foreach (var extraWindow in atkOverlayState.ExtraWindows)
                    {
                        _windowOverlayMap.Remove(extraWindow);
                    }
                }

                atkOverlayState.Clear();
                Overlays.Remove(windowName);
            }
        }

        public void RemoveOverlay(IAtkOverlayState overlayState)
        {
            RemoveOverlay(overlayState.WindowName);
        }

        public void ClearOverlays()
        {
            foreach (var overlay in _overlays)
            {
                RemoveOverlay(overlay.Value);
            }
        }

        private Dictionary<WindowName, WindowName> _windowOverlayMap = new();
        private Dictionary<WindowName, bool> _windowStatuses = new();
        private void GameUiOnUiVisibilityChanged(WindowName windowname, bool? windowstate)
        {
            if (PluginService.PluginLoaded)
            {
                var extraWindowChange = false;
                if (_windowOverlayMap.ContainsKey(windowname) && windowstate != null)
                {
                    var originalWindow = windowname;
                    _windowStatuses[windowname] = windowstate.Value;
                    windowname = _windowOverlayMap[windowname];
                    extraWindowChange = true;
                }
                //As some overlays represent multiple windows we need to track each window and whether or not it's active, once the entire "group" of windows is active we update the overlay accordingly
                if (_overlays.ContainsKey(windowname))
                {
                    if (windowstate != null && !extraWindowChange)
                    {
                        _windowStatuses[windowname] = windowstate.Value;
                    }
                    var overlay = _overlays[windowname];
                    if (overlay.ExtraWindows != null)
                    {
                        var extraWindowsReady = overlay.ExtraWindows.All(c => _windowStatuses.ContainsKey(c) && _windowStatuses[c] == windowstate) && _windowStatuses.ContainsKey(windowname) && _windowStatuses[windowname] == windowstate;
                        if (!extraWindowsReady)
                        {
                            return;
                        }

                        if (windowstate != null)
                        {
                            PluginLog.Verbose("All extra windows of " + windowname +
                                              " are now active, entire overlay is now " +
                                              (windowstate.Value ? "visible" : "invisible"));
                        }
                    }
                    if (windowstate == true)
                    {
                        RefreshOverlayStates();
                    }
                    if (windowstate.HasValue && windowstate.Value)
                    {
                        SetupUpdateHook(overlay);
                        if (_lastState != null && !overlay.HasState)
                        {
                            PluginLog.Verbose("Applying last known state to " + windowname);
                            overlay.UpdateState(_lastState);
                        }
                    }

                    if (windowstate.HasValue && !windowstate.Value)
                    {
                        PluginLog.Verbose("Applying empty state to " + windowname);
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
                _frameworkService.Update -= FrameworkOnUpdate;
                _filterService.FilterTableRefreshed -= FilterServiceOnFilterTableRefreshed;
                _filterService.FilterRecalculated -= FilterServiceOnFilterRecalculated;
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
        
        ~OverlayService()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( _disposed == false )
            {
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}