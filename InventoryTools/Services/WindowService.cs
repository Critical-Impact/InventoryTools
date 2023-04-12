using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Window = InventoryTools.Ui.Window;

namespace InventoryTools.Services
{
    public class WindowService : IDisposable
    {
        private readonly WindowSystem windowSystem = new("AllaganTools");

        private IFilterService _filterService;
        public WindowService(IFilterService filterService)
        {
            _filterService = filterService;
            _filterService.FilterRemoved += FilterServiceAddedRemoved;
            _filterService.FilterAdded += FilterServiceAddedRemoved;
            _filterService.FilterRepositioned += FilterServiceOnFilterRepositioned;
            _filterService.FilterInvalidated += FilterServiceOnFilterInvalidated;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }

        private void FilterServiceOnFilterInvalidated(FilterConfiguration configuration)
        {
            if (_windows.ContainsKey(CraftsWindow.AsKey))
            {
                _windows[CraftsWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(FiltersWindow.AsKey))
            {
                _windows[FiltersWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(ConfigurationWindow.AsKey))
            {
                _windows[ConfigurationWindow.AsKey].Invalidate();
            }
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            RestoreSavedWindows();
        }

        private ConcurrentDictionary<string, Window> _windows = new();
        public ConcurrentDictionary<string, Window> Windows => _windows;

        private void RestoreSavedWindows()
        {
            var openWindows = ConfigurationManager.Config.OpenWindows;
            ConfigurationManager.Config.OpenWindows = new HashSet<string>();
            foreach (var openWindow in openWindows)
            {
                OpenGuessWindow(openWindow);
            }
        }

        public bool OpenItemWindow(uint itemId)
        {
            var asKey = ItemWindow.AsKey(itemId);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var itemWindow = new ItemWindow(itemId);
            return AddWindow(itemWindow);
        }

        public bool OpenENpcWindow(uint eNpcId)
        {
            var asKey = ENpcWindow.AsKey(eNpcId);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var eNpcWindow = new ENpcWindow(eNpcId);
            return AddWindow(eNpcWindow);
        }

        public bool OpenDutyWindow(uint contentFinderConditionId)
        {
            var asKey = DutyWindow.AsKey(contentFinderConditionId);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var dutyWindow = new DutyWindow(contentFinderConditionId);
            return AddWindow(dutyWindow);
        }

        public bool OpenAirshipWindow(uint airshipExplorationPointId)
        {
            var asKey = AirshipWindow.AsKey(airshipExplorationPointId);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var airshipWindow = new AirshipWindow(airshipExplorationPointId);
            return AddWindow(airshipWindow);
        }

        public bool OpenSubmarineWindow(uint submarineExplorationPointId)
        {
            var asKey = SubmarineWindow.AsKey(submarineExplorationPointId);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var submarineWindow = new SubmarineWindow(submarineExplorationPointId);
            return AddWindow(submarineWindow);
        }

        public bool OpenFilterWindow(string filterKey)
        {
            var asKey = FilterWindow.AsKey(filterKey);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var filterWindow = new FilterWindow(filterKey);
            return AddWindow(filterWindow);
        }

        public bool HasFilterWindowOpen
        {
            get
            {
                return _windows.Any(c => c.Value.SelectedConfiguration != null && c.Value.IsOpen);
            }
        }

        public WindowSystem WindowSystem => windowSystem;

        public CraftsWindow GetCraftsWindow()
        {
            var asKey = CraftsWindow.AsKey;
            if (_windows.ContainsKey(asKey) && _windows[asKey] is CraftsWindow)
            {
                return (CraftsWindow)_windows[asKey];
            }
            var craftsWindow = new CraftsWindow();
            AddWindow(craftsWindow);
            return craftsWindow;
        }

        public T GetWindow<T>(string windowName) where T: Window, new()
        {
            if (_windows.ContainsKey(windowName) && _windows[windowName] is T)
            {
                return (T)_windows[windowName];
            }
            var newWindow = new T();
            AddWindow(newWindow);
            return newWindow;
        }

        public bool ToggleCraftsWindow()
        {
            return ToggleWindow<CraftsWindow>(CraftsWindow.AsKey);
        }

        public bool OpenGuessWindow(string windowName)
        {
            if (windowName == CraftsWindow.AsKey)
            {
                return OpenWindow<CraftsWindow>(CraftsWindow.AsKey);
            }

            if (windowName == FiltersWindow.AsKey)
            {
                return OpenWindow<FiltersWindow>(FiltersWindow.AsKey);
            }
            #if DEBUG
            if (windowName == DebugWindow.AsKey)
            {
                return OpenWindow<DebugWindow>(DebugWindow.AsKey);
            }
            #endif
            if (windowName == HelpWindow.AsKey)
            {
                return OpenWindow<HelpWindow>(HelpWindow.AsKey);
            }

            if (windowName == ConfigurationWindow.AsKey)
            {
                return OpenWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
            }

            if (windowName == DutiesWindow.AsKey)
            {
                return OpenWindow<DutiesWindow>(DutiesWindow.AsKey);
            }

            if (windowName == BNpcWindow.AsKey)
            {
                return OpenWindow<BNpcWindow>(BNpcWindow.AsKey);
            }

            if (windowName == AirshipsWindow.AsKey)
            {
                return OpenWindow<AirshipsWindow>(AirshipsWindow.AsKey);
            }

            if (windowName == SubmarinesWindow.AsKey)
            {
                return OpenWindow<SubmarinesWindow>(SubmarinesWindow.AsKey);
            }

            foreach (var config in ConfigurationManager.Config.FilterConfigurations)
            {
                if (windowName == FilterWindow.AsKey(config.Key))
                {
                    return OpenFilterWindow(config.Key);
                }
            }

            return false;
        }

        public bool OpenCraftsWindow()
        {
            return OpenWindow<CraftsWindow>(CraftsWindow.AsKey);
        }

        public bool ToggleTetrisWindow()
        {
            return ToggleWindow<TetrisWindow>(TetrisWindow.AsKey);
        }
        #if DEBUG
        public bool ToggleDebugWindow()
        {
            return ToggleWindow<DebugWindow>(DebugWindow.AsKey);
        }
        #endif

        public bool ToggleWindow<T>(string windowKey) where T: Window, new()
        {
            var asKey = windowKey;
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }
            var window = new T();
            return AddWindow(window);
        }


        public bool OpenWindow<T>(string windowKey, bool refocus = true) where T: Window, new()
        {
            var asKey = windowKey;
            if (_windows.ContainsKey(asKey))
            {
                if (_windows[asKey].IsOpen)
                {
                    if (refocus)
                    {
                        _windows[asKey].BringToFront();
                    }
                }
                else
                {
                    _windows[asKey].Open();
                }
                return true;
            }
            var window = new T();
            return AddWindow(window);
        }

        private bool AddWindow(Window window)
        {
            if (_windows.TryAdd(window.Key, window))
            {
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                window.Open();
                return true;
            }

            return false;
        }

        private void WindowOnOpened(string windowKey)
        {
            if(!ConfigurationManager.Config.OpenWindows.Contains(windowKey))
            {
                ConfigurationManager.Config.OpenWindows.Add(windowKey);
            }
            var currentWindow = Windows[windowKey];
            if (currentWindow.SavePosition)
            {
                if (ConfigurationManager.Config.SavedWindowPositions.ContainsKey(currentWindow.GenericKey))
                {
                    currentWindow.Position = ConfigurationManager.Config.SavedWindowPositions[currentWindow.GenericKey];
                    currentWindow.PositionCondition = ImGuiCond.Appearing;
                }
            }
            PluginService.OverlayService.RefreshOverlayStates();
        }

        private void WindowOnClosed(string windowKey)
        {
            if(ConfigurationManager.Config.OpenWindows.Contains(windowKey))
            {
                ConfigurationManager.Config.OpenWindows.Remove(windowKey);
            }

            var currentWindow = Windows[windowKey];
            if (currentWindow.SavePosition)
            {
                bool hasOtherWindowOpen = false;
                foreach (var window in Windows)
                {
                    if (window.Value != currentWindow && window.Value.GenericKey == currentWindow.GenericKey &&
                        window.Value.IsOpen)
                    {
                        hasOtherWindowOpen = true;
                    }
                }
                
                if (hasOtherWindowOpen == false)
                {
                    ConfigurationManager.Config.SavedWindowPositions[currentWindow.GenericKey] =
                        currentWindow.CurrentPosition;
                }

            }

            PluginService.OverlayService.RefreshOverlayStates();
        }
        
        public bool ToggleConfigurationWindow()
        {
            return ToggleWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
        }
        
        public bool ToggleDutiesWindow()
        {
            return ToggleWindow<DutiesWindow>(DutiesWindow.AsKey);
        }
        
        public bool ToggleAirshipsWindow()
        {
            return ToggleWindow<AirshipsWindow>(AirshipsWindow.AsKey);
        }
        
        public bool ToggleMobWindow()
        {
            return ToggleWindow<BNpcWindow>(BNpcWindow.AsKey);
        }

        public bool ToggleHelpWindow()
        {
            return ToggleWindow<HelpWindow>(HelpWindow.AsKey);
        }

        public bool ToggleFiltersWindow()
        {
            return ToggleWindow<FiltersWindow>(FiltersWindow.AsKey);
        }

        public bool ToggleSubmarinesWindow()
        {
            return ToggleWindow<SubmarinesWindow>(SubmarinesWindow.AsKey);
        }

        public bool CloseFilterWindows()
        {
            foreach (var window in _windows)
            {
                if (window.Value is FilterWindow)
                {
                    window.Value.Close();
                }
            }

            return true;
        }

        public bool ToggleFilterWindow(string filterKey)
        {
            var asKey = FilterWindow.AsKey(filterKey);
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Toggle();
                return true;
            }

            return OpenFilterWindow(filterKey);
        }
        
        private void FilterServiceOnFilterRepositioned(FilterConfiguration configuration)
        {
            if (_windows.ContainsKey(CraftsWindow.AsKey))
            {
                _windows[CraftsWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(FiltersWindow.AsKey))
            {
                _windows[FiltersWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(ConfigurationWindow.AsKey))
            {
                _windows[ConfigurationWindow.AsKey].Invalidate();
            }
        }

        private void FilterServiceAddedRemoved(FilterConfiguration configuration)
        {
            if (_windows.ContainsKey(CraftsWindow.AsKey))
            {
                _windows[CraftsWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(FiltersWindow.AsKey))
            {
                _windows[FiltersWindow.AsKey].Invalidate();
            }
            if (_windows.ContainsKey(ConfigurationWindow.AsKey))
            {
                _windows[ConfigurationWindow.AsKey].Invalidate();
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
                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
                _filterService.FilterRepositioned -= FilterServiceOnFilterRepositioned;
                _filterService.FilterRemoved -= FilterServiceAddedRemoved;
                _filterService.FilterAdded -= FilterServiceAddedRemoved;
                _filterService.FilterInvalidated -= FilterServiceOnFilterInvalidated;
                foreach (var window in _windows)
                {
                    window.Value.Opened -= WindowOnOpened;
                    window.Value.Closed -= WindowOnClosed;
                }
            }
            _disposed = true;         
        }
        
            
        ~WindowService()
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