using System;
using System.Collections.Generic;
using System.Timers;
using CriticalCommonLib;
using Dalamud.Plugin.Ipc;

namespace InventoryTools.Logic
{
    public class WotsitIpc: IDisposable, IWotsitIpc
    {
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


        public WotsitIpc()
        {
            InitForWotsit();
            _wotsitAvailable = Service.Interface.GetIpcSubscriber<bool>("FA.Available");
            _wotsitAvailable.Subscribe(FaAvailable);
            
            PluginService.FilterService.FilterAdded += FilterAddedRemoved;
            PluginService.FilterService.FilterRemoved += FilterAddedRemoved;
            PluginService.FilterService.FilterModified += FilterChanged;

            _delayTimer = new Timer(5000);
            _delayTimer.Elapsed += DelayTimerOnElapsed;
            _delayTimer.Enabled = true;
        }

        private void FaAvailable()
        {
            PluginService.FrameworkService.RunOnTick(InitForWotsit);
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
                Service.Log.Error("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
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
                Service.Log.Error("Something went wrong while trying to unregister and reregister with wotsit's IPC.");
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
                    Service.Log.Verbose(e , "Could not register with Wotsit IPC. This is normal if you do not have it installed.");
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

                foreach (var filter in PluginService.FilterService.FiltersList)
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
                        Service.Log.Verbose(e , "Could not register with Wotsit IPC. This is normal if you do not have it installed.");
                    }
                }
            }

            Service.Log.Debug($"Registered {_wotsitToggleFilterGuids.Count} filters with Wotsit");
        }

        public void WotsitInvoke(string guid)
        {
            PluginService.FrameworkService.RunOnFrameworkThread(() =>
            {
                if (_wotsitToggleFilterGuids.TryGetValue(guid, out var filter))
                {
                    PluginService.FilterService.ToggleActiveBackgroundFilter(filter);
                }
            });
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
                    _wotsitAvailable.Unsubscribe(FaAvailable);
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
        
        ~WotsitIpc()
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