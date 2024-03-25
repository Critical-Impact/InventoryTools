using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Window = InventoryTools.Ui.Window;

namespace InventoryTools.Services
{
    public class WindowService : DisposableMediatorSubscriberBase, IHostedService
    {
        private readonly WindowSystem windowSystem = new("AllaganTools");

        private readonly Func<Type, uint, UintWindow> _uintWindowFactory;
        private readonly Func<Type, string, StringWindow> _stringWindowFactory;
        private readonly Func<Type, GenericWindow> _genericWindowFactory;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly MediatorService _mediatorService;

        public WindowService(ILogger<WindowService> logger, MediatorService mediatorService, IEnumerable<Window> windows, Func<Type, uint, UintWindow> uintWindowFactory, Func<Type, string, StringWindow> stringWindowFactory, Func<Type, GenericWindow> genericWindowFactory, InventoryToolsConfiguration configuration) : base(logger, mediatorService)
        {
            _windows = windows.ToDictionary(c => c.GetType(), c => c);
            _uintWindowFactory = uintWindowFactory;
            _stringWindowFactory = stringWindowFactory;
            _genericWindowFactory = genericWindowFactory;
            _configuration = configuration;
            _mediatorService = mediatorService;
        }

        private void UintWindowMessage(OpenUintWindowMessage obj)
        {
            OpenWindow(obj.windowType, obj.windowId);
        }

        private void GenericWindowMessage(OpenGenericWindowMessage obj)
        {
            OpenWindow(obj.windowType);
        }

        public void UpdateRespectCloseHotkey(Type windowType, bool newSetting)
        {
            foreach (var window in _allWindows)
            {
                if (window.GetType() == windowType)
                {
                    window.RespectCloseHotkey = newSetting;
                }
            }
        }

        private List<Window> _allWindows = new();
        private ConcurrentDictionary<(Type,string), IWindow> _stringWindows = new();
        private ConcurrentDictionary<(Type,uint), IWindow> _uintWindows = new();
        private ConcurrentDictionary<Type, IWindow> _genericWindows = new();

        private MethodInfo? _openWindowMethod;
        private readonly Dictionary<Type,Window> _windows;


        private void RestoreSavedWindows()
        {
            var openWindows = _configuration.OpenWindows;
            _configuration.OpenWindows = new HashSet<string>();
            foreach (var openWindow in openWindows)
            {
                Assembly asm = typeof(WindowService).Assembly;
                Type? type = asm.GetType(openWindow);

                if (type != null)
                {
                    try
                    {
                        var newWindow = _genericWindowFactory.Invoke(type);
                        newWindow.Initialize();
                        AddWindow(newWindow);
                        newWindow.Open();

                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Could not load saved window. Perhaps it was removed.");
                    }
                }
            }
        }

        public bool HasFilterWindowOpen
        {
            get
            {
                return _allWindows.Any(c => c.SelectedConfiguration != null && c.IsOpen);
            }
        }

        public WindowSystem WindowSystem => windowSystem;

        public T GetWindow<T>() where T: GenericWindow 
        {
            if (_genericWindows.ContainsKey(typeof(T)))
            {
                return (T)_genericWindows[typeof(T)];
            }
            ;
            var newWindow = _genericWindowFactory.Invoke(typeof(T));
            newWindow.Initialize();
            AddWindow(newWindow);
            return (T)newWindow;
        }

        public GenericWindow GetWindow(Type type)
        {
            if (_genericWindows.ContainsKey(type))
            {
                return (GenericWindow)_genericWindows[type];
            }
            ;
            var newWindow = _genericWindowFactory.Invoke(type);
            newWindow.Initialize();
            AddWindow(type, newWindow);
            return newWindow;
        }
        
        public UintWindow GetWindow(Type type, uint windowId)
        {
            if (_uintWindows.ContainsKey((type,windowId)))
            {
                return (UintWindow)_uintWindows[(type,windowId)];
            }
            ;
            var newWindow = _uintWindowFactory.Invoke(type, windowId);
            newWindow.Initialize(windowId);
            AddWindow(type, newWindow, windowId);
            return newWindow;
        }
        
        public StringWindow GetWindow(Type type, string windowId)
        {
            if (_stringWindows.ContainsKey((type,windowId)))
            {
                return (StringWindow)_stringWindows[(type,windowId)];
            }
            ;
            var newWindow = _stringWindowFactory.Invoke(type, windowId);
            newWindow.Initialize(windowId);
            AddWindow(type, newWindow, windowId);
            return newWindow;
        }

        public T GetWindow<T>(uint windowId) where T: UintWindow
        {
            if(_uintWindows.ContainsKey((typeof(T),windowId)) && _uintWindows[(typeof(T),windowId)] is T)
            {
                return (T)_uintWindows[(typeof(T),windowId)];
            }
            var newWindow = _uintWindowFactory.Invoke(typeof(T), windowId);
            newWindow.Initialize(windowId);
            AddWindow(newWindow, windowId);
            return (T)newWindow;
        }

        public T GetWindow<T>(string windowId) where T: StringWindow
        {
            if(_stringWindows.ContainsKey((typeof(T),windowId)) && _stringWindows[(typeof(T),windowId)] is T)
            {
                return (T)_stringWindows[(typeof(T),windowId)];
            }
            var newWindow = _stringWindowFactory.Invoke(typeof(T), windowId);
            newWindow.Initialize(windowId);
            AddWindow(newWindow, windowId);
            return (T)newWindow;
        }

        public bool ToggleWindow<T>() where T: GenericWindow
        {
            GetWindow<T>().Toggle();
            return true;
        }
        
        public bool ToggleWindow<T>(uint windowId) where T: UintWindow
        {
            GetWindow<T>(windowId).Toggle();
            return true;
        }

        public bool ToggleWindow<T>(string windowId) where T : StringWindow
        {
            GetWindow<T>(windowId).Toggle();
            return true;
        }

        public bool ToggleWindow(Type window)
        {
            GetWindow(window).Toggle();
            return true;
        }

        public bool ToggleWindow(Type window, string windowId)
        {
            GetWindow(window, windowId).Toggle();
            return true;
        }

        public bool ToggleWindow(Type window, uint windowId)
        {
            GetWindow(window, windowId).Toggle();
            return true;
        }
        public bool OpenWindow(Type type,bool refocus = true)
        {
            var window = GetWindow(type);
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }
            return true;
        }
        public bool OpenWindow(Type type, uint windowId, bool refocus = true)
        {
            var window = GetWindow(type, windowId);
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }
            return true;
        }
        public bool OpenWindow(Type type, string windowId, bool refocus = true)
        {
            var window = GetWindow(type, windowId);
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }
            return true;
        }
        public bool OpenWindow<T>(bool refocus = true) where T: GenericWindow
        {
            var window = GetWindow<T>();
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }

            return true;
        }
        public bool OpenWindow<T>(uint windowId, bool refocus = true) where T: UintWindow
        {
            var window = GetWindow<T>(windowId);
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }

            return true;
        }
        public bool OpenWindow<T>(string windowId, bool refocus = true) where T: StringWindow
        {
            var window = GetWindow<T>(windowId);
            if (window.IsOpen)
            {
                window.BringToFront();
            }
            else
            {
                window.Open();
            }

            return true;
        }
        
        private bool CloseWindow(Type windowType)
        {
            if (_genericWindows.ContainsKey(windowType))
            {
                _genericWindows[windowType].Close();
                return true;
            }
            return false;
        }
        
        private bool CloseWindow(Type windowType, uint windowId)
        {
            if (_uintWindows.ContainsKey((windowType,windowId)))
            {
                _uintWindows[(windowType,windowId)].Close();
                return true;
            }
            return false;
        }
        
        private bool CloseWindow(Type windowType, string windowId)
        {
            if (_stringWindows.ContainsKey((windowType,windowId)))
            {
                _stringWindows[(windowType,windowId)].Close();
                return true;
            }
            return false;
        }
        
        private bool CloseWindows()
        {
            foreach (var window in _allWindows)
            {
                window.Close();
            }

            return true;
        }
        
        private bool CloseWindows(Type type)
        {
            foreach (var window in _allWindows)
            {
                if (type.IsInstanceOfType(window))
                {
                    window.Close();
                }
            }

            return true;
        }
        
        private bool AddWindow(Type windowType, GenericWindow window)
        {
            window.Logger = Logger;
            if (_genericWindows.TryAdd(windowType, window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }
        
        private bool AddWindow(Type windowType, UintWindow window, uint windowId)
        {
            window.Logger = Logger;
            if (_uintWindows.TryAdd((windowType,windowId), window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }
        
        private bool AddWindow(Type windowType, StringWindow window, string windowId)
        {
            window.Logger = Logger;
            if (_stringWindows.TryAdd((windowType,windowId), window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }

        private bool AddWindow<T>(T window) where T: GenericWindow
        {
            window.Logger = Logger;
            if (_genericWindows.TryAdd(window.GetType(), window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }

        private bool AddWindow<T>(T window, uint windowId) where T: UintWindow
        {
            window.Logger = Logger;
            if (_uintWindows.TryAdd((typeof(T),windowId), window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }

        private bool AddWindow<T>(T window, string windowId) where T: StringWindow
        {
            window.Logger = Logger;
            if (_stringWindows.TryAdd((typeof(T),windowId), window))
            {
                _allWindows.Add(window);
                windowSystem.AddWindow(window);
                window.Closed += WindowOnClosed;
                window.Opened += WindowOnOpened;
                return true;
            }
            return false;
        }

        private void WindowOnOpened(IWindow window)
        {
            if(!_configuration.OpenWindows.Contains(window.GetType().ToString()))
            {
                _configuration.OpenWindows.Add(window.GetType().ToString());
            }
            if (window.SavePosition)
            {
                if (_configuration.SavedWindowPositions.ContainsKey(window.GetType().ToString()))
                {
                    window.SetPosition(_configuration.SavedWindowPositions[window.GetType().ToString()], true);
                }
            }
            MediatorService.Publish(new OverlaysRequestRefreshMessage());
        }

        private void WindowOnClosed(IWindow window)
        {
            if(_configuration.OpenWindows.Contains(window.GetType().ToString()))
            {
                _configuration.OpenWindows.Remove(window.GetType().ToString());
            }

            if (window.SavePosition)
            {
                bool hasOtherWindowOpen = false;
                //Check to see if there are any other instances of the window open, if so don't save the one that was just closed's position
                foreach (var openWindow in _allWindows)
                {
                    if (window != openWindow && window.Key == openWindow.GenericKey &&
                        window.IsOpen)
                    {
                        hasOtherWindowOpen = true;
                    }
                }
                
                if (hasOtherWindowOpen == false)
                {
                    _configuration.SavedWindowPositions[window.GetType().ToString()] = window.CurrentPosition;
                }

            }
            MediatorService.Publish(new OverlaysRequestRefreshMessage());
        }

                
        
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var window in _allWindows)
            {
                window.Opened -= WindowOnOpened;
                window.Closed -= WindowOnClosed;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
            _mediatorService.Subscribe(this, new Action<ToggleGenericWindowMessage>(ToggleGenericWindow) );
            _mediatorService.Subscribe(this, new Action<ToggleUintWindowMessage>(ToggleUintWindowMessage) );
            _mediatorService.Subscribe(this, new Action<ToggleStringWindowMessage>(ToggleStringWindow));
            _mediatorService.Subscribe(this, new Action<OpenGenericWindowMessage>(GenericWindowMessage) );
            _mediatorService.Subscribe(this, new Action<OpenUintWindowMessage>(UintWindowMessage) );
            _mediatorService.Subscribe(this, new Action<OpenStringWindowMessage>(OpenStringWindow) );
            _mediatorService.Subscribe(this, new Action<CloseWindowMessage>(CloseWindow) );
            _mediatorService.Subscribe(this, new Action<CloseUintWindowMessage>(CloseUintWindow) );
            _mediatorService.Subscribe(this, new Action<CloseStringWindowMessage>(CloseStringWindow) );
            _mediatorService.Subscribe(this, new Action<CloseWindowsByTypeMessage>(CloseWindowsByType) );
            _mediatorService.Subscribe(this, new Action<CloseWindowsMessage>(CloseWindows) );
            _mediatorService.Subscribe(this, new Action<OpenSavedWindowsMessage>(OpenSavedWindows) );
            _mediatorService.Subscribe(this, new Action<UpdateWindowRespectClose>(close => UpdateRespectCloseHotkey(close.windowType, close.newSetting)) );
            
            return Task.CompletedTask;
        }

        private void OpenSavedWindows(OpenSavedWindowsMessage obj)
        {
            RestoreSavedWindows();
        }

        private void CloseWindows(CloseWindowsMessage obj)
        {
            CloseWindows();
        }

        private void CloseWindowsByType(CloseWindowsByTypeMessage obj)
        {
            CloseWindows(obj.windowType);
        }

        private void CloseStringWindow(CloseStringWindowMessage obj)
        {
            CloseWindow(obj.windowType, obj.windowId);
        }

        private void CloseUintWindow(CloseUintWindowMessage obj)
        {
            CloseWindow(obj.windowType, obj.windowId);
        }

        private void CloseWindow(CloseWindowMessage obj)
        {
            CloseWindow(obj.windowType);
        }

        private void OpenStringWindow(OpenStringWindowMessage obj)
        {
            OpenWindow(obj.windowType, obj.windowId);
        }

        private void ToggleStringWindow(ToggleStringWindowMessage obj)
        {
            ToggleWindow(obj.windowType, obj.windowId);
        }

        private void ToggleUintWindowMessage(ToggleUintWindowMessage obj)
        {
            ToggleWindow(obj.windowType, obj.windowId);
        }

        private void ToggleGenericWindow(ToggleGenericWindowMessage obj)
        {
            ToggleWindow(obj.windowType);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}