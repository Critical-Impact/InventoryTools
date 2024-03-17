using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.GameUi;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services
{
    public class OverlayService : IDisposable, IOverlayService
    {
        private readonly IFilterService _filterService;
        private readonly IAddonLifecycle _addonLifecycle;
        private readonly IFramework _frameworkService;
        private readonly Dictionary<string, bool> _windowVisible = new();
        private readonly HashSet<string> _windowsToTrack = new();
        private readonly Dictionary<string, IAtkOverlayState> _overlays = new();
        private FilterState? _lastState;

        public OverlayService(IAddonLifecycle addonLifecycle, IFilterService filterService, IFramework frameworkService)
        {
            _addonLifecycle = addonLifecycle;
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
            _addonLifecycle.RegisterListener(AddonEvent.PostRefresh, PostRefresh);
            _addonLifecycle.RegisterListener(AddonEvent.PostSetup, PostSetup);
            _addonLifecycle.RegisterListener(AddonEvent.PreFinalize, PreFinalize);
            _addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate,PostRequestedUpdate );
            frameworkService.Update += FrameworkOnUpdate;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }
        
        public FilterState? LastState => _lastState;

        public Dictionary<string, IAtkOverlayState> Overlays
        {
            get => _overlays;
        }

        private void PostRequestedUpdate(AddonEvent type, AddonArgs args)
        {
            if (_windowsToTrack.Contains(args.AddonName))
            {
                DrawOverlays(args.AddonName);
            }
        }

        private void PreFinalize(AddonEvent type, AddonArgs args)
        {
            //Window destroyed
            if (_windowsToTrack.Contains(args.AddonName))
            {
                _windowVisible[args.AddonName] = false;
                DrawOverlays(args.AddonName);
            }
        }

        private void PostSetup(AddonEvent type, AddonArgs args)
        {
            //Window loaded
            if (_windowsToTrack.Contains(args.AddonName))
            {
                _windowVisible[args.AddonName] = true;
                UpdateState(_lastState);
                DrawOverlays(args.AddonName);
            }
        }

        private unsafe void PostRefresh(AddonEvent type, AddonArgs args)
        {
            //Window shown/hidden/needs redraw
            var addon = (AtkUnitBase*)args.Addon;
            if (_windowsToTrack.Contains(args.AddonName))
            {
                var newState = addon->IsVisible;
                var oldState = _windowVisible.ContainsKey(args.AddonName) && _windowVisible[args.AddonName];
                if (oldState != newState)
                {
                    _windowVisible[args.AddonName] = newState;
                    UpdateState(_lastState);
                }

                DrawOverlays(args.AddonName);
            }
        }

        private void DrawOverlays(string addonName)
        {
            foreach (var overlay in Overlays)
            {
                if (addonName == overlay.Key)
                {
                    if (overlay.Value.NeedsStateRefresh)
                    {
                        overlay.Value.UpdateState(_lastState);
                        overlay.Value.NeedsStateRefresh = false;
                    }
                    if (_windowVisible.ContainsKey(overlay.Key) && _windowVisible[overlay.Key] && overlay.Value.Draw())
                    {
                        
                    }
                }
            }
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
            Service.Log.Debug("Overlays refreshing, active filter is " + (activeFilter?.Name ?? "no filter"));
            if (activeFilter != null && _filterService.HasFilterTable(activeFilter))
            {
                UpdateState(new FilterState(activeFilter){FilterTable = _filterService.GetFilterTable(activeFilter)});
            }
            else if (activeBackgroundFilter != null)
            {
                UpdateState(new FilterState(activeBackgroundFilter));
            }
            else
            {
                UpdateState(null);
            }
        }

        private void FrameworkOnUpdate(IFramework framework)
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

        public void UpdateState(FilterState? filterState)
        {
            foreach (var overlay in _overlays)
            {
                overlay.Value.UpdateState(filterState);
                _lastState = filterState;
            }
        }


        public void AddOverlay(IAtkOverlayState overlayState)
        {
            _windowsToTrack.Add(overlayState.WindowName.ToString());
            if (overlayState.ExtraWindows != null)
            {
                foreach (var extraWindow in overlayState.ExtraWindows)
                {
                    _windowsToTrack.Add(overlayState.WindowName.ToString());
                }
            }
            
            
            if (!Overlays.ContainsKey(overlayState.WindowName.ToString()))
            {
                Overlays.Add(overlayState.WindowName.ToString(), overlayState);
                overlayState.Setup();
                overlayState.Draw();
            }
            else
            {
                Service.Log.Error("Attempted to add an overlay that is already registered.");
            }
        }

        public void RemoveOverlay(WindowName windowName)
        {
            if (Overlays.ContainsKey(windowName.ToString()))
            {
                var atkOverlayState = Overlays[windowName.ToString()];
                atkOverlayState.Clear();
                Overlays.Remove(windowName.ToString());
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
                _addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, PostRefresh);
                _addonLifecycle.UnregisterListener(AddonEvent.PostSetup, PostSetup);
                _addonLifecycle.UnregisterListener(AddonEvent.PreFinalize, PreFinalize);                
                _addonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate, PostRequestedUpdate);                
                _frameworkService.Update -= FrameworkOnUpdate;
                _filterService.FilterTableRefreshed -= FilterServiceOnFilterTableRefreshed;
                _filterService.FilterRecalculated -= FilterServiceOnFilterRecalculated;
                ClearOverlays();
                _filterService.FilterModified -= FilterServiceOnFilterModified;
                _filterService.UiFilterToggled -= FilterServiceOnFilterToggled;
                _filterService.BackgroundFilterToggled -= FilterServiceOnFilterToggled;
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
                Service.Log.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}