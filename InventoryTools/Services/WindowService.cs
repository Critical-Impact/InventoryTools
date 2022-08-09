using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic;
using InventoryTools.Ui;

namespace InventoryTools.Services
{
    public class WindowService : IDisposable
    {
        private FilterService _filterService;
        public WindowService(FilterService filterService)
        {
            _filterService = filterService;
            _filterService.FilterRemoved += FilterServiceAddedRemoved;
            _filterService.FilterAdded += FilterServiceAddedRemoved;
            _filterService.FilterRepositioned += FilterServiceOnFilterRepositioned;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            RestoreSavedWindows();
        }

        private ConcurrentDictionary<string, IWindow> _windows = new();
        public ConcurrentDictionary<string, IWindow> Windows => _windows;

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
                return _windows.Any(c => c.Value.SelectedConfiguration != null && c.Value.Visible);
            }
        }

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


        public bool OpenWindow<T>(string windowKey) where T: Window, new()
        {
            var asKey = windowKey;
            if (_windows.ContainsKey(asKey))
            {
                _windows[asKey].Open();
                return true;
            }
            var window = new T();
            return AddWindow(window);
        }

        private bool AddWindow(IWindow window)
        {
            if (_windows.TryAdd(window.Key, window))
            {
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
            PluginService.OverlayService.RefreshOverlayStates();
        }

        private void WindowOnClosed(string windowKey)
        {
            if(ConfigurationManager.Config.OpenWindows.Contains(windowKey))
            {
                ConfigurationManager.Config.OpenWindows.Remove(windowKey);
            }
            PluginService.OverlayService.RefreshOverlayStates();
        }
        
        public bool ToggleConfigurationWindow()
        {
            return ToggleWindow<ConfigurationWindow>(ConfigurationWindow.AsKey);
        }

        public bool ToggleHelpWindow()
        {
            return ToggleWindow<HelpWindow>(HelpWindow.AsKey);
        }

        public bool ToggleFiltersWindow()
        {
            return ToggleWindow<FiltersWindow>(FiltersWindow.AsKey);
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
        }

        public void Dispose()
        {
            PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
            _filterService.FilterRepositioned -= FilterServiceOnFilterRepositioned;
            _filterService.FilterRemoved -= FilterServiceAddedRemoved;
            _filterService.FilterAdded -= FilterServiceAddedRemoved;
            foreach (var window in _windows)
            {
                window.Value.Opened -= WindowOnOpened;
                window.Value.Closed -= WindowOnClosed;
            }
        }
    }
}