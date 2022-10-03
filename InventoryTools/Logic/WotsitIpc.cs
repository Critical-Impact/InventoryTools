using System;
using System.Collections.Generic;
using System.Timers;
using CriticalCommonLib;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;

namespace InventoryTools.Logic
{
    public class WotsitIpc : IDisposable
    {
        private const string IpcDisplayName = "Allagan Tools";
        private const uint WotsitIconId = 32;

        private ICallGateSubscriber<string, string, string, uint, string>? _wotsitRegister;
        private ICallGateSubscriber<string, bool>? _wotsitUnregister;
        private ICallGateSubscriber<string, bool>? _callGateSubscriber;
        private Dictionary<string, FilterConfiguration> _wotsitToggleFilterGuids = new();
        private Dictionary<FilterConfiguration, string> _wotsitFilterNames = new();
        private bool _wotsItRegistered = false;
        private Timer? _delayTimer = null;


        public WotsitIpc()
        {
            try
            {
                InitForWotsit();
            }
            catch (Exception)
            {
                _wotsItRegistered = false;
                // ignored
            }

            var wotsitAvailable = Service.Interface.GetIpcSubscriber<bool>("FA.Available");
            wotsitAvailable.Subscribe(InitForWotsit);
            
            PluginService.FilterService.FilterAdded += FilterAddedRemoved;
            PluginService.FilterService.FilterRemoved += FilterAddedRemoved;
            PluginService.FilterService.FilterModified += FilterChanged;

            _delayTimer = new Timer(5000);
            _delayTimer.Elapsed += DelayTimerOnElapsed;
            _delayTimer.Enabled = true;
        }

        private void DelayTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            _delayTimer?.Stop();
            if (!_wotsItRegistered)
            {
                InitForWotsit();
            }
        }

        private void FilterAddedRemoved(FilterConfiguration configuration)
        {
            try
            {
                InitForWotsit();
            }
            catch (Exception)
            {
                PluginLog.Error("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
            }
        }

        private void FilterChanged(FilterConfiguration configuration)
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
                PluginLog.Error("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
            }
        }

        private void InitForWotsit()
        {
            if (_wotsItRegistered && _wotsitUnregister != null)
            {
                _wotsitUnregister?.InvokeFunc(IpcDisplayName);
                _wotsitFilterNames.Clear();
                _wotsitToggleFilterGuids.Clear();
            }

            if (_wotsitRegister == null)
            {
                _wotsitRegister =
                    Service.Interface.GetIpcSubscriber<string, string, string, uint, string>("FA.RegisterWithSearch");
            }


            if (_wotsitUnregister == null)
            {
                _wotsitUnregister = Service.Interface.GetIpcSubscriber<string, bool>("FA.UnregisterAll");
            }

            if (_callGateSubscriber == null)
            {
                _callGateSubscriber = Service.Interface.GetIpcSubscriber<string, bool>("FA.Invoke");
                _callGateSubscriber.Subscribe(WotsitInvoke);
            }

            _wotsItRegistered = true;
            RegisterFilters();
        }

        private void RegisterFilters()
        {
            if (_wotsitRegister != null)
            {
                _wotsitToggleFilterGuids = new Dictionary<string, FilterConfiguration>();

                foreach (var filter in PluginService.FilterService.FiltersList)
                {
                    var guid = _wotsitRegister.InvokeFunc(IpcDisplayName, $"Toggle Filter - {filter.Name}",
                        $"Toggle the filter on/off {filter.Name} as a background filter. ", WotsitIconId);
                    _wotsitToggleFilterGuids.Add(guid, filter);
                    _wotsitFilterNames.Add(filter, filter.Name);
                }
            }

            PluginLog.Debug($"Registered {_wotsitToggleFilterGuids.Count} filters with Wotsit");
        }

        private void WotsitInvoke(string guid)
        {
            if (_wotsitToggleFilterGuids.TryGetValue(guid, out var filter))
            {
                PluginService.FilterService.ToggleActiveBackgroundFilter(filter);
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
                try
                {
                    _wotsitUnregister?.InvokeFunc(IpcDisplayName);
                }
                catch (Exception)
                {
                    // Wotsit was not installed or too early version
                }
                PluginService.FilterService.FilterAdded -= FilterAddedRemoved;
                PluginService.FilterService.FilterRemoved -= FilterAddedRemoved;
                PluginService.FilterService.FilterModified -= FilterChanged;
                if (_delayTimer != null)
                {
                    _delayTimer.Elapsed -= DelayTimerOnElapsed;
                    _delayTimer?.Dispose();
                }
            }
            _disposed = true;         
        }
    }
}