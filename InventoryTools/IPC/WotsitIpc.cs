using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CriticalCommonLib;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace InventoryTools.IPC
{
    using Dalamud.Plugin;

    public class WotsitIpc: IWotsitIpc
    {
        private readonly CompendiumWotsitSetting _compendiumWotsitSetting;
        private readonly MediatorService _mediatorService;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly IEnumerable<ICompendiumType> _compendiumTypes;
        private readonly ConfigurationManagerService _configurationManagerService;
        private readonly IDalamudPluginInterface _dalamudPluginInterface;
        private readonly IListService _listService;
        private readonly IFramework _framework;
        public ILogger<WotsitIpc> Logger { get; }
        private const string IpcDisplayName = "Allagan Tools";
        private const uint WotsitIconId = 32;

        private ICallGateSubscriber<string, string, string, uint, string>? _wotsitRegister;
        private ICallGateSubscriber<string, bool>? _wotsitUnregister;
        private ICallGateSubscriber<string, string, bool>? _wotsitUnregisterOne;
        private ICallGateSubscriber<string, bool>? _callGateSubscriber;
        private ICallGateSubscriber<bool> _wotsitAvailable;
        private Dictionary<string, string> _wotsitToggleFilterGuids = new();
        private Dictionary<string, ICompendiumType> _wotsitCompendiumIds = new();
        private Dictionary<string, string> _wotsitFilterNames = new();
        private bool? _compendiumState;
        private bool _wotsItRegistered = false;
        private Timer? _delayTimer = null;


        public WotsitIpc(CompendiumWotsitSetting compendiumWotsitSetting, MediatorService mediatorService, InventoryToolsConfiguration configuration, IEnumerable<ICompendiumType> compendiumTypes, ConfigurationManagerService configurationManagerService, ILogger<WotsitIpc> logger, IDalamudPluginInterface dalamudPluginInterface, IListService listService, IFramework framework)
        {
            _compendiumWotsitSetting = compendiumWotsitSetting;
            _mediatorService = mediatorService;
            _configuration = configuration;
            _compendiumTypes = compendiumTypes.Where(c => c.ShowInListing).OrderBy(c => c.Plural);
            _configurationManagerService = configurationManagerService;
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
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Something went wrong while trying to unregister and reregister with wotsit's IPC.");
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
            catch (Exception exception)
            {
                Logger.LogWarning(exception, "Something went wrong while trying to unregister and reregister with wotsit's IPC.");
            }
        }

        public void InitForWotsit()
        {
            if (_wotsitUnregister == null)
            {
                _wotsitUnregister = _dalamudPluginInterface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
            }
            if (_wotsitUnregister == null)
            {
                _wotsitUnregisterOne = _dalamudPluginInterface.GetIpcSubscriber<string, string, bool>("FA.UnregisterOne");
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
            if (_compendiumState == null)
            {
                SetupCompendium();
            }
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

        public void SetupCompendium()
        {
            if (this._compendiumState is null or false && this._compendiumWotsitSetting.CurrentValue(_configuration))
            {
                RegisterCompendium();
            }
            if (this._compendiumState is true && !this._compendiumWotsitSetting.CurrentValue(_configuration))
            {
                UnRegisterCompendium();
            }
        }

        public void RegisterCompendium()
        {
            if (_wotsitRegister != null)
            {
                _wotsitCompendiumIds = new Dictionary<string, ICompendiumType>();

                foreach (var compendiumType in _compendiumTypes)
                {
                    try
                    {
                        var guid = _wotsitRegister.InvokeFunc(IpcDisplayName, "Compendium - " + compendiumType.Plural,
                            $"Show the compendium list for " + compendiumType.Plural, WotsitIconId);
                        _wotsitCompendiumIds.Add(guid, compendiumType);
                    }
                    catch (Exception e)
                    {
                        Logger.LogTrace(e , "Could not register with Wotsit IPC. This is normal if you do not have it installed.");
                    }
                }

                _compendiumState = true;
            }

            Logger.LogDebug("Registered {CompendiumCount} compendium types with wotsit", _compendiumTypes.Count());
        }

        public void UnRegisterCompendium()
        {
            if (_wotsitUnregisterOne != null)
            {
                foreach (var compendiumType in _wotsitCompendiumIds)
                {
                    try
                    {
                        _wotsitUnregisterOne.InvokeFunc(IpcDisplayName, compendiumType.Key);
                    }
                    catch (Exception e)
                    {
                        Logger.LogTrace(e , "Could not unregister with Wotsit IPC. This is normal if you do not have it installed.");
                    }
                }
                _wotsitCompendiumIds.Clear();
                _compendiumState = false;
            }

            Logger.LogDebug("Unregistered {CompendiumCount} compendium types with wotsit", _compendiumTypes.Count());
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
                if (_wotsitCompendiumIds.TryGetValue(guid, out var compendiumType))
                {
                    _mediatorService.Publish(new ToggleCompendiumListMessage(compendiumType));
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
            _configurationManagerService.ConfigurationChanged += ConfigurationManagerServiceOnConfigurationChanged;

            _delayTimer = new Timer(5000);
            _delayTimer.Elapsed += DelayTimerOnElapsed;
            _delayTimer.Enabled = true;

            return Task.CompletedTask;
        }

        private void ConfigurationManagerServiceOnConfigurationChanged()
        {
            SetupCompendium();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {Type} ({This})", GetType().Name, this);
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
            _configurationManagerService.ConfigurationChanged -= ConfigurationManagerServiceOnConfigurationChanged;
            _listService.ListAdded -= ListAddedRemoved;
            _listService.ListRemoved -= ListAddedRemoved;
            _listService.ListConfigurationChanged -= ListChanged;
            if (_delayTimer != null)
            {
                _delayTimer.Elapsed -= DelayTimerOnElapsed;
                _delayTimer?.Dispose();
            }
            Logger.LogTrace("Stopped service {Type} ({This})", GetType().Name, this);
            return Task.CompletedTask;
        }
    }


}