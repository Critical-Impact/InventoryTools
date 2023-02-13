using System.Reflection;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Ui;
using Lumina;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace InventoryToolsMock;

public class MockPlugin
{
    public HelpWindow HelpWindow;
    private List<Window> Windows;
    private ICharacterMonitor _characterMonitor;
    private IInventoryMonitor _inventoryMonitor;
    private IInventoryScanner _inventoryScanner;
    private FilterService _filterService;
    private MockFrameworkService _frameworkService;
    private MockIconService _iconService;
    private MockUniversalis _universalis;
    private WindowService _windowService;
    private MockGameUiManager _gameUiManager;
    private MockMarketCache _marketCache;
    private MockPluginInterfaceService _mockPluginInterfaceService;
    private PluginLogic _pluginLogic;
    private MockDataService _dataService;
    private OverlayService _overlayService;
    private MockCraftMonitor _craftMonitor;
    private MockChatUtilities _chatUtilities;
    private MockGameInterface _gameInterface;

    public MockPlugin(string gameDirectory, string configDirectory, string configFile, string? inventoriesFile)
    {
        ConfigurationManager.Config = new InventoryToolsConfiguration();
        var levelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = LogEventLevel.Verbose,
        };
        var lumina = new Lumina.GameData( gameDirectory, new LuminaOptions()
        {
            PanicOnSheetChecksumMismatch = false
        } );


        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .MinimumLevel.ControlledBy(levelSwitch)
            .CreateLogger();
        
        _characterMonitor = new MockCharacterMonitor();
        _craftMonitor = new MockCraftMonitor();
        _inventoryScanner = new MockInventoryScanner();
        _frameworkService = new MockFrameworkService();
        _chatUtilities = new MockChatUtilities();
        _inventoryMonitor = new InventoryMonitor(_characterMonitor, _craftMonitor, _inventoryScanner, _frameworkService );
        _iconService = new MockIconService(lumina);
        _universalis = new MockUniversalis();
        _gameUiManager = new MockGameUiManager();
        _gameInterface = new MockGameInterface();
        _marketCache = new MockMarketCache();
        _mockPluginInterfaceService = new MockPluginInterfaceService(new FileInfo(configFile), new DirectoryInfo(configDirectory));
        _dataService = new MockDataService(lumina);
        Service.ExcelCache = new ExcelCache(lumina);
        _pluginLogic = new PluginLogic(true);
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
            PluginLogic = _pluginLogic,
            ChatUtilities =  _chatUtilities,
            GameInterface = _gameInterface
        }, false);

        ConfigurationManager.LoadFromFile(configFile, inventoriesFile);
        PluginService.InventoryMonitor.LoadExistingData(ConfigurationManager.Config.GetSavedInventory());
        PluginService.CharacterMonitor.LoadExistingRetainers(ConfigurationManager.Config.GetSavedRetainers());
        _filterService = new FilterService(_characterMonitor, _inventoryMonitor);
        _windowService = new WindowService(_filterService);
        _overlayService = new OverlayService(_filterService, _gameUiManager, _frameworkService);
        PluginService.InitaliseExplicit(new MockServices()
        {
            FilterService = _filterService,
            WindowService = _windowService,
            OverlayService = _overlayService,
            DataService = _dataService
        });
        PluginService.PluginLogic.RunMigrations();
            
        if (ConfigurationManager.Config.FirstRun)
        {
            PluginService.PluginLogic.LoadDefaultData();
            ConfigurationManager.Config.FirstRun = false;
        }
        _windowService.OpenWindow<MockWindow>(MockWindow.AsKey);
    }

    private HashSet<string> WindowStates = new HashSet<string>();
    public void Draw()
    {
        foreach (var window in _windowService.Windows)
        {
            DrawWindow(window.Value);
        }
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
}