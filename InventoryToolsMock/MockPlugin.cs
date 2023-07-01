using System.Diagnostics;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;
using Lumina;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Veldrid;
using Window = InventoryTools.Ui.Window;

namespace InventoryToolsMock;

public class MockPlugin : IDisposable
{
    public HelpWindow HelpWindow;
    private List<Window> Windows;
    private ICharacterMonitor _characterMonitor;
    private IInventoryMonitor _inventoryMonitor;
    private IInventoryScanner _inventoryScanner;
    private FilterService _filterService;
    public MockFrameworkService _frameworkService;
    private MockIconService _iconService;
    private MockUniversalis _universalis;
    private WindowService _windowService;
    public MockGameUiManager _gameUiManager;
    private MockMarketCache _marketCache;
    private MockPluginInterfaceService _mockPluginInterfaceService;
    private PluginLogic _pluginLogic;
    private MockDataService _dataService;
    private OverlayService _overlayService;
    private MockCraftMonitor _craftMonitor;
    private MockChatUtilities _chatUtilities;
    private MockGameInterface _gameInterface;
    private FileDialogManager _fileDialogManager;
    private IMobTracker _mockMobTracker;
    private ITooltipService _tooltipService;
    private ICommandService _commandService;
    private IKeyStateService _keyStateService;
    private IHotkeyService _hotkeyService;
    private InventoryHistory _inventoryHistory;
    private WindowSystem _windowSystem;

    public MockPlugin(GameData gameData, string configDirectory)
    {
        var configFile = Path.Combine(configDirectory, "InventoryTools.json");        
        var configFolder = Path.Combine(configDirectory,"InventoryTools");             
        ConfigurationManager.Config = new InventoryToolsConfiguration();
        var levelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = LogEventLevel.Verbose,
        };
        var lumina = gameData;

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .MinimumLevel.ControlledBy(levelSwitch)
            .CreateLogger();
        
        
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _characterMonitor = new MockCharacterMonitor();
        _craftMonitor = new MockCraftMonitor();
        _inventoryScanner = new MockInventoryScanner();
        _frameworkService = new MockFrameworkService();
        _chatUtilities = new MockChatUtilities();
        _inventoryMonitor = new InventoryMonitor(_characterMonitor, _craftMonitor, _inventoryScanner, _frameworkService );
        _inventoryHistory = new InventoryHistory(_inventoryMonitor);
        _iconService = new MockIconService(lumina);
        _universalis = new MockUniversalis();
        _gameUiManager = new MockGameUiManager();
        _gameInterface = new MockGameInterface();
        _marketCache = new MockMarketCache();
        _mockPluginInterfaceService = new MockPluginInterfaceService(new FileInfo(configFile), new DirectoryInfo(configFolder));
        _dataService = new MockDataService(lumina);
        _fileDialogManager = new FileDialogManager();
        _mockMobTracker = new MockMobTracker();
        _tooltipService = new MockTooltipService();
        _commandService = new MockCommandService();
        _keyStateService = new MockKeyStateService();
        _hotkeyService = new HotkeyService(_frameworkService, _keyStateService);
        Service.ExcelCache = new ExcelCache(lumina);
        Service.ExcelCache.PreCacheItemData();
        PluginService.InitaliseExplicit(new MockServices()
        {
            CharacterMonitor = _characterMonitor,
            InventoryMonitor = _inventoryMonitor,
            InventoryScanner = _inventoryScanner,
            CraftMonitor = _craftMonitor,
            FrameworkService = _frameworkService,
            IconService = _iconService,
            Universalis = _universalis,
            GameUiManager = _gameUiManager,
            MarketCache = _marketCache,
            PluginInterfaceService = _mockPluginInterfaceService,
            ChatUtilities =  _chatUtilities,
            GameInterface = _gameInterface,
            FileDialogManager = _fileDialogManager,
            MobTracker = _mockMobTracker,
            TooltipService = _tooltipService,
            CommandService =  _commandService,
            HotkeyService = _hotkeyService,
            KeyStateService = _keyStateService,
            InventoryHistory = _inventoryHistory
            
        }, false);
        ConfigurationManager.Load(configFile);
        var inventories = ConfigurationManager.LoadInventory();
        PluginService.CharacterMonitor.LoadExistingRetainers(ConfigurationManager.Config.GetSavedRetainers());
        PluginService.InventoryMonitor.LoadExistingData(inventories);

        PluginService.InventoryHistory.LoadExistingHistory(ConfigurationManager.LoadHistoryFromCsv(out _));
        PluginService.InitaliseExplicit(new MockServices()
        {
        }, false);
        _filterService = new FilterService(_characterMonitor, _inventoryMonitor, _inventoryHistory);
        _windowService = new WindowService(_filterService);
        _overlayService = new OverlayService(_filterService, _gameUiManager, _frameworkService);
        PluginService.InitaliseExplicit(new MockServices()
        {
            FilterService = _filterService,
            WindowService = _windowService,
            OverlayService = _overlayService,
            DataService = _dataService
        }, false);
        _pluginLogic = new PluginLogic();
        PluginService.InitaliseExplicit(new MockServices()
        {
            PluginLogic = _pluginLogic,
        }, true);
        PluginService.PluginLogic.RunMigrations();
            
        if (ConfigurationManager.Config.FirstRun)
        {
            PluginService.PluginLogic.LoadDefaultData();
            ConfigurationManager.Config.FirstRun = false;
        }

        stopWatch.Stop();
        PluginLog.Verbose("Allagan Tools has finished loading. Total load time was " + stopWatch.Elapsed.TotalSeconds + " seconds.");
        Program._window.KeyDown += WindowOnKeyDown;
        Program._window.KeyUp += WindowOnKeyUp;
        _windowService.OpenWindow<MockWindow>(MockWindow.AsKey);
    }

    private void WindowOnKeyDown(KeyEvent keyEvent)
    {
        var keyState = keyEvent.ToKeyState();
        _keyStateService[keyState] = true;
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            _keyStateService[VirtualKey.SHIFT] = true;
        }
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _keyStateService[VirtualKey.CONTROL] = true;
        }
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            _keyStateService[VirtualKey.MENU] = true;
        }
    }

    private void WindowOnKeyUp(KeyEvent keyEvent)
    {
        var keyState = keyEvent.ToKeyState();
        _keyStateService[keyState] = false;
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            _keyStateService[VirtualKey.SHIFT] = false;
        }
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Control))
        {
            _keyStateService[VirtualKey.CONTROL] = false;
        }
        if (keyEvent.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            _keyStateService[VirtualKey.MENU] = false;
        }
    }

    private HashSet<string> WindowStates = new HashSet<string>();
    public void Draw()
    {
        foreach (var window in _windowService.Windows)
        {
            DrawWindow(window.Value);
        }
        PluginService.FileDialogManager.Draw();
    }

    public void DrawWindow(Window window)
    {
        window.PreOpenCheck();
        if (!window.IsOpen)
        {
            return;
        }

        if (!WindowStates.Contains(window.Key))
        {
            WindowStates.Add(window.Key);
            window.OnOpen();
        }
        window.Update();
        if (!window.DrawConditions())
            return;
        bool flag1 = !string.IsNullOrEmpty(window.Namespace);
        if (flag1)
            ImGui.PushID(window.Namespace);
        window.PreDraw();
        ApplyConditionals(window);
        if (window.ForceMainWindow || true)
            ImGuiHelpers.ForceNextWindowMainViewport();

        bool isFocused = window.IsFocused;
        if (isFocused)
            ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, ImGui.GetStyle().Colors[11]);
        var internalIsOpen = window.IsOpen;
        if ((window.ShowCloseButton
                ? (ImGui.Begin(window.WindowName, ref internalIsOpen, window.Flags) ? 1 : 0)
                : (ImGui.Begin(window.WindowName, window.Flags) ? 1 : 0)) != 0)
            window.Draw();
        if (isFocused)
            ImGui.PopStyleColor();
        if (internalIsOpen != window.IsOpen)
        {
            if (WindowStates.Contains(window.Key))
            {
                WindowStates.Remove(window.Key);
            }

            window.Close();
            window.OnClose();
        }


        ImGui.End();
        window.PostDraw();
        if (!flag1)
            return;
        ImGui.PopID();
    }
    
    private void ApplyConditionals(Window window)
    {
        if (window.Position.HasValue)
        {
            var pos = window.Position.Value;

            if (window.ForceMainWindow)
                pos += ImGuiHelpers.MainViewport.Pos;

            ImGui.SetNextWindowPos(pos, window.PositionCondition);
        }

        if (window.Size.HasValue)
        {
            ImGui.SetNextWindowSize(window.Size.Value * ImGuiHelpers.GlobalScale, window.SizeCondition);
        }

        if (window.Collapsed.HasValue)
        {
            ImGui.SetNextWindowCollapsed(window.Collapsed.Value, window.CollapsedCondition);
        }

        if (window.SizeConstraints.HasValue)
        {
            ImGui.SetNextWindowSizeConstraints(window.SizeConstraints.Value.MinimumSize * ImGuiHelpers.GlobalScale, window.SizeConstraints.Value.MaximumSize * ImGuiHelpers.GlobalScale);
        }

        if (window.BgAlpha.HasValue)
        {
            ImGui.SetNextWindowBgAlpha(window.BgAlpha.Value);
        }
    }

    public void Dispose()
    {
        _characterMonitor.Dispose();
        _inventoryMonitor.Dispose();
        _inventoryScanner.Dispose();
        _filterService.Dispose();
        _iconService.Dispose();
        _universalis.Dispose();
        _windowService.Dispose();
        _gameUiManager.Dispose();
        _marketCache.Dispose();
        _pluginLogic.Dispose();
        _craftMonitor.Dispose();
        _gameInterface.Dispose();
        Service.ExcelCache.Dispose();
    }
}