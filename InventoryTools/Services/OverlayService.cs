using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Overlays;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services
{
    public class OverlayService : MediatorSubscriberBase, IOverlayService
    {
        private readonly IListService _listService;
        private readonly TableService _tableService;
        private readonly IAddonLifecycle _addonLifecycle;
        private readonly IFramework _frameworkService;
        private readonly Func<FilterConfiguration, FilterState> _filterStateFactory;
        private readonly ILogger<OverlayService> _logger;
        private readonly Dictionary<string, bool> _windowVisible = new();
        private readonly HashSet<string> _windowsToTrack = new();
        private readonly List<IGameOverlay> _overlays = new();
        private FilterState? _lastState;

        public OverlayService(ILogger<OverlayService> logger, MediatorService mediatorService, IAddonLifecycle addonLifecycle, IListService listService,TableService tableService, IFramework frameworkService, Func<FilterConfiguration, FilterState> filterStateFactory, IEnumerable<IGameOverlay> gameOverlays) : base(logger, mediatorService)
        {
            _addonLifecycle = addonLifecycle;
            _listService = listService;
            _tableService = tableService;
            _frameworkService = frameworkService;
            _filterStateFactory = filterStateFactory;
            _logger = logger;

            foreach (var overlay in gameOverlays)
            {
                AddOverlay(overlay);
            }
        }

        public FilterState? LastState => _lastState;

        public List<IGameOverlay> Overlays
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
                if (!overlay.Enabled) continue;
                if (addonName == overlay.WindowName.ToString())
                {
                    if (overlay.NeedsStateRefresh)
                    {
                        overlay.UpdateState(_lastState);
                        overlay.NeedsStateRefresh = false;
                    }
                    if (_windowVisible.ContainsKey(overlay.WindowName.ToString()) && _windowVisible[overlay.WindowName.ToString()] && overlay.Draw())
                    {

                    }
                }
            }
        }

        private void TableRefreshed(RenderTableBase tableBase)
        {
            RefreshOverlayStates();
        }

        private void ListServiceOnListToggled(FilterConfiguration configuration, bool newstate)
        {
            RefreshOverlayStates();
        }

        private void ListServiceOnListModified(FilterConfiguration configuration)
        {
            RefreshOverlayStates();
        }

        public void RefreshOverlayStates()
        {
            if (!_frameworkService.IsInFrameworkUpdateThread)
            {
                _frameworkService.RunOnFrameworkThread(RefreshOverlayStates);
                return;
            }
            var activeFilter = _listService.GetActiveUiList(false);
            var activeBackgroundFilter = _listService.GetActiveBackgroundList();
            _logger.LogDebug($"Overlays refreshing, active list: {activeFilter?.Name ?? "no list"}, background list: {activeBackgroundFilter?.Name ?? "no list"}");
            if (activeFilter != null && _tableService.HasListTable(activeFilter))
            {
                var newState = _filterStateFactory.Invoke(activeFilter);
                newState.FilterTable = _tableService.GetListTable(activeFilter);
                UpdateState(newState);
            }
            else if (activeBackgroundFilter != null)
            {
                var newState = _filterStateFactory.Invoke(activeBackgroundFilter);
                UpdateState(newState);
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
                if (!overlay.Enabled) continue;
                if (overlay.NeedsStateRefresh)
                {
                    overlay.UpdateState(_lastState);
                    overlay.NeedsStateRefresh = false;
                }

                if (overlay.HasAddon)
                {
                    overlay.Update();
                }
            }
        }

        public void UpdateState(FilterState? filterState)
        {
            _lastState = filterState;
            foreach (var overlay in _overlays)
            {
                if (!overlay.Enabled) continue;
                overlay.UpdateState(filterState);
            }
        }

        public void EnableOverlay(IGameOverlay overlayState)
        {
            overlayState.Enabled = true;
            overlayState.NeedsStateRefresh = true;
        }

        public void DisableOverlay(IGameOverlay overlayState)
        {
            overlayState.Enabled = false;
            overlayState.Clear();
        }


        public void AddOverlay(IGameOverlay overlay)
        {
            _windowsToTrack.Add(overlay.WindowName.ToString());
            if (overlay.ExtraWindows != null)
            {
                foreach (var extraWindow in overlay.ExtraWindows)
                {
                    _windowsToTrack.Add(extraWindow.ToString());
                }
            }

            if (!Overlays.Contains(overlay))
            {
                Overlays.Add(overlay);
                overlay.Setup();
                overlay.Draw();
            }
            else
            {
                _logger.LogError("Attempted to add an overlay that is already registered.");
            }
        }

        public void ClearOverlays()
        {
            foreach (var overlay in _overlays)
            {
                overlay.Clear();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            MediatorService.Subscribe<OverlaysRequestRefreshMessage>(this, RefreshRequested);
            _listService.ListConfigurationChanged += ListServiceOnListModified;
            _listService.UiListToggled += ListServiceOnListToggled;
            _listService.BackgroundListToggled += ListServiceOnListToggled;
            _tableService.TableRefreshed += TableRefreshed;
            _addonLifecycle.RegisterListener(AddonEvent.PostRefresh, PostRefresh);
            _addonLifecycle.RegisterListener(AddonEvent.PostSetup, PostSetup);
            _addonLifecycle.RegisterListener(AddonEvent.PreFinalize, PreFinalize);
            _addonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate,PostRequestedUpdate );
            _frameworkService.Update += FrameworkOnUpdate;
            return Task.CompletedTask;
        }

        private void RefreshRequested(OverlaysRequestRefreshMessage obj)
        {
            RefreshOverlayStates();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _addonLifecycle.UnregisterListener(AddonEvent.PostRefresh, PostRefresh);
            _addonLifecycle.UnregisterListener(AddonEvent.PostSetup, PostSetup);
            _addonLifecycle.UnregisterListener(AddonEvent.PreFinalize, PreFinalize);
            _addonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate, PostRequestedUpdate);
            _frameworkService.Update -= FrameworkOnUpdate;
            _tableService.TableRefreshed -= TableRefreshed;
            ClearOverlays();
            _listService.ListConfigurationChanged -= ListServiceOnListModified;
            _listService.UiListToggled -= ListServiceOnListToggled;
            _listService.BackgroundListToggled -= ListServiceOnListToggled;
            UnsubscribeAll();
            return Task.CompletedTask;
        }
    }
}