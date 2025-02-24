using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CriticalCommonLib;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace InventoryTools.IPC
{
    using Dalamud.Plugin;

    public class WotsitIpc: IWotsitIpc
    {
        private readonly IDalamudPluginInterface _dalamudPluginInterface;
        private readonly IListService _listService;
        private readonly IFramework _framework;
        public ILogger<WotsitIpc> Logger { get; }
        private const string IpcDisplayName = "Allagan Tools";
        private const uint WotsitIconId = 32;

        private ICallGateSubscriber<string, string, string, uint, string>? _wotsitRegister;
        private ICallGateSubscriber<string, bool>? _wotsitUnregister;
        private ICallGateSubscriber<string, bool>? _callGateSubscriber;
        private ICallGateSubscriber<bool> _wotsitAvailable;
        private Dictionary<string, string> _wotsitToggleFilterGuids = new();
        private Dictionary<string, string> _wotsitFilterNames = new();
        private bool _wotsItRegistered = false;
        private Timer? _delayTimer = null;


        public WotsitIpc(ILogger<WotsitIpc> logger, IDalamudPluginInterface dalamudPluginInterface, IListService listService, IFramework framework)
        {
            _dalamudPluginInterface = dalamudPluginInterface;
            _listService = listService;
            _framework = framework;
            Logger = logger;
        }

        private void FaAvailable()
        {
            _framework.RunOnTick(InitForWotsit);
        }

        private void DelayTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            _delayTimer?.Stop();
            if (!_wotsItRegistered)
            {
                InitForWotsit();
            }
        }

        private void ListAddedRemoved(FilterConfiguration configuration)
        {
            try
            {
                InitForWotsit();
            }
            catch (Exception)
            {
                Logger.LogError("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
            }
        }

        private void ListChanged(FilterConfiguration configuration)
        {
            try
            {
                if (_wotsitFilterNames.ContainsKey(configuration.Key) && _wotsitFilterNames[configuration.Key] != configuration.Name)
                {
                    InitForWotsit();
                }
            }
            catch (Exception)
            {
                Logger.LogError("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
            }
        }

        public void InitForWotsit()
        {
            if (_wotsitUnregister == null)
            {
                _wotsitUnregister = _dalamudPluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
            }

            if (_wotsitRegister == null)
            {
                _wotsitRegister =
                    _dalamudPluginInterface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");
            }

            if (_callGateSubscriber == null)
            {
                _callGateSubscriber = _dalamudPluginInterface.GetIpcSubscriber<string, bool>("FA.Invoke");
                _callGateSubscriber.Subscribe(WotsitInvoke);
            }

            if (_wotsitUnregister != null)
            {
                try
                {
                    _wotsitUnregister?.InvokeFunc(IpcDisplayName);
                    _wotsitFilterNames.Clear();
                    _wotsitToggleFilterGuids.Clear();
                }
                catch (Exception e)
                {
                    Logger.LogTrace(e , "Could not register with Wotsit IPC. This is normal if you do not have it installed.");
                    _wotsItRegistered = false;
                    return;
                }
            }

            _wotsItRegistered = true;
            RegisterFilters();
        }

        public void RegisterFilters()
        {
            if (_wotsitRegister != null)
            {
                _wotsitToggleFilterGuids = new Dictionary<string, string>();

                foreach (var filter in _listService.Lists)
                {
                    try
                    {
                        var guid = _wotsitRegister.InvokeFunc(IpcDisplayName, $"Toggle Filter - {filter.Name}",
                            $"Toggle the filter on/off {filter.Name} as a background filter. ", WotsitIconId);
                        _wotsitToggleFilterGuids.Add(guid, filter.Key);
                        _wotsitFilterNames.Add(filter.Key, filter.Name);
                    }
                    catch (Exception e)
                    {
                        Logger.LogTrace(e , "Could not register with Wotsit IPC. This is normal if you do not have it installed.");
                    }
                }
            }

            Logger.LogDebug($"Registered {_wotsitToggleFilterGuids.Count} lists with Wotsit");
        }

        public void WotsitInvoke(string guid)
        {
            _framework.RunOnFrameworkThread(() =>
            {
                if (_wotsitToggleFilterGuids.TryGetValue(guid, out var filterKey))
                {
                    var filter = _listService.GetListByKey(filterKey);
                    if (filter != null)
                    {
                        _listService.ToggleActiveBackgroundList(filter);
                    }
                }
            });
        }

        public void Dispose()
        {

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);

            InitForWotsit();
            _wotsitAvailable = _dalamudPluginInterface.GetIpcSubscriber<bool>("FA.Available");
            _wotsitAvailable.Subscribe(FaAvailable);

            _listService.ListAdded += ListAddedRemoved;
            _listService.ListRemoved += ListAddedRemoved;
            _listService.ListConfigurationChanged += ListChanged;

            _delayTimer = new Timer(5000);
            _delayTimer.Elapsed += DelayTimerOnElapsed;
            _delayTimer.Enabled = true;

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
            try
            {
                _wotsitAvailable.Unsubscribe(FaAvailable);
                _wotsitUnregister?.InvokeFunc(IpcDisplayName);
                _callGateSubscriber?.Unsubscribe(WotsitInvoke);
            }
            catch (Exception)
            {
                // Wotsit was not installed or too early version
            }
            _listService.ListAdded -= ListAddedRemoved;
            _listService.ListRemoved -= ListAddedRemoved;
            _listService.ListConfigurationChanged -= ListChanged;
            if (_delayTimer != null)
            {
                _delayTimer.Elapsed -= DelayTimerOnElapsed;
                _delayTimer?.Dispose();
            }
            return Task.CompletedTask;
        }
    }


}