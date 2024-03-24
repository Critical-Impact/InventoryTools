using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CriticalCommonLib;
using Dalamud.Plugin.Ipc;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace InventoryTools.Logic
{
    public class WotsitIpc: IWotsitIpc
    {
        private readonly IListService _listService;
        public ILogger<WotsitIpc> Logger { get; }
        private const string IpcDisplayName = "Allagan Tools";
        private const uint WotsitIconId = 32;

        private ICallGateSubscriber<string, string, string, uint, string>? _wotsitRegister;
        private ICallGateSubscriber<string, bool>? _wotsitUnregister;
        private ICallGateSubscriber<string, bool>? _callGateSubscriber;
        private ICallGateSubscriber<bool> _wotsitAvailable;
        private Dictionary<string, FilterConfiguration> _wotsitToggleFilterGuids = new();
        private Dictionary<FilterConfiguration, string> _wotsitFilterNames = new();
        private bool _wotsItRegistered = false;
        private Timer? _delayTimer = null;


        public WotsitIpc(ILogger<WotsitIpc> logger, IListService listService)
        {
            _listService = listService;
            Logger = logger;
        }

        private void FaAvailable()
        {
            Service.Framework.RunOnTick(InitForWotsit);
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
                if (_wotsitFilterNames.ContainsKey(configuration) && _wotsitFilterNames[configuration] != configuration.Name)
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
                _wotsitUnregister = Service.Interface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
            }
            
            if (_wotsitRegister == null)
            {
                _wotsitRegister =
                    Service.Interface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");
            }

            if (_callGateSubscriber == null)
            {
                _callGateSubscriber = Service.Interface.GetIpcSubscriber<string, bool>("FA.Invoke");
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
                _wotsitToggleFilterGuids = new Dictionary<string, FilterConfiguration>();

                foreach (var filter in _listService.Lists)
                {
                    try
                    {
                        var guid = _wotsitRegister.InvokeFunc(IpcDisplayName, $"Toggle Filter - {filter.Name}",
                            $"Toggle the filter on/off {filter.Name} as a background filter. ", WotsitIconId);
                        _wotsitToggleFilterGuids.Add(guid, filter);
                        _wotsitFilterNames.Add(filter, filter.Name);
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
            Service.Framework.RunOnFrameworkThread(() =>
            {
                if (_wotsitToggleFilterGuids.TryGetValue(guid, out var filter))
                {
                    _listService.ToggleActiveBackgroundList(filter);
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
            _wotsitAvailable = Service.Interface.GetIpcSubscriber<bool>("FA.Available");
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
            try
            {
                _wotsitAvailable.Unsubscribe(FaAvailable);
                _wotsitUnregister?.InvokeFunc(IpcDisplayName);
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