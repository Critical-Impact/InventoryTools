using System.Diagnostics;
using System.Windows.Input;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using DalaMock;
using DalaMock.Dalamud;
using DalaMock.Extensions;
using DalaMock.Interfaces;
using DalaMock.Mock;
using DalaMock.Shared.Interfaces;
using DalaMock.Windows;
using Dalamud;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
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
using Veldrid.Sdl2;
using Window = InventoryTools.Ui.Window;

namespace InventoryToolsMock;

public class MockPlugin : IMockPlugin, IDisposable
{
    private List<Window> Windows;
    private ICharacterMonitor? _characterMonitor;
    private IInventoryMonitor? _inventoryMonitor;
    private IInventoryScanner? _inventoryScanner;
    private FilterService? _filterService;
    private MockIconService? _iconService;
    private MockUniversalis? _universalis;
    private WindowService? _windowService;
    private MockGameUiManager? _gameUiManager;
    private MockMarketCache? _marketCache;
    private PluginLogic? _pluginLogic;
    private OverlayService? _overlayService;
    private MockCraftMonitor? _craftMonitor;
    private MockChatUtilities? _chatUtilities;
    private MockGameInterface? _gameInterface;
    private FileDialogManager? _fileDialogManager;
    private IMobTracker? _mockMobTracker;
    private ITooltipService? _tooltipService;
    private IHotkeyService? _hotkeyService;
    private InventoryHistory? _inventoryHistory;
    private IFont? _font;
    private bool _isStarted;

    public void Draw()
    {
        if (_isStarted)
        {
            if (_windowService != null)
            {
                foreach (var window in _windowService.WindowSystem.Windows)
                {
                    window.AllowPinning = false;
                    window.AllowClickthrough = false;
                }
                _windowService.WindowSystem.Draw();
            }
            PluginService.FileDialogManager.Draw();
        }
    }

    public void Dispose()
    {
        _characterMonitor?.Dispose();
        _inventoryMonitor?.Dispose();
        _inventoryScanner?.Dispose();
        _filterService?.Dispose();
        _iconService?.Dispose();
        _universalis?.Dispose();
        _windowService?.Dispose();
        _gameUiManager?.Dispose();
        _marketCache?.Dispose();
        _pluginLogic?.Dispose();
        _craftMonitor?.Dispose();
        _gameInterface?.Dispose();
        Service.SeTime.Dispose();
        Service.ExcelCache.Dispose();
        ConfigurationManager.Save();
    }

    public bool IsStarted => _isStarted;

    public void Start(MockProgram program, MockService mockService, MockPluginInterfaceService mockPluginInterfaceService)
    {
        Service.Interface = mockPluginInterfaceService;
        var gameData = Service.Data.GameData;
        var clientLanguage = ClientLanguage.English;
        var configFile = Path.Combine(mockPluginInterfaceService.ConfigDirectory.FullName, mockPluginInterfaceService.ConfigFile.FullName);        
        ConfigurationManager.Config = new InventoryToolsConfiguration();
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var mockTeleporter = new MockTeleporterIpc(Service.Log);
        _font = new MockFont();
        _characterMonitor = new MockCharacterMonitor();
        _craftMonitor = new MockCraftMonitor();
        _inventoryScanner = new MockInventoryScanner();
        _chatUtilities = new MockChatUtilities(Service.Log);
        _inventoryMonitor = new InventoryMonitor(_characterMonitor, _craftMonitor, _inventoryScanner, Service.Framework );
        _inventoryHistory = new InventoryHistory(_inventoryMonitor);
        _iconService = new MockIconService(gameData, program);
        _universalis = new MockUniversalis();
        _gameUiManager = new MockGameUiManager();
        _gameInterface = new MockGameInterface(Service.Log);
        _marketCache = new MockMarketCache();
        _fileDialogManager = new FileDialogManager();
        _mockMobTracker = new MockMobTracker();
        _tooltipService = new MockTooltipService();
        _hotkeyService = new HotkeyService(Service.Framework, Service.KeyState);
        Service.ExcelCache = new ExcelCache(gameData);
        Service.ExcelCache.PreCacheItemData();
        Service.SeTime = new MockSeTime(); 
        PluginService.InitaliseExplicit(new MockServices()
        {
            CharacterMonitor = _characterMonitor,
            InventoryMonitor = _inventoryMonitor,
            InventoryScanner = _inventoryScanner,
            CraftMonitor = _craftMonitor,
            IconService = _iconService,
            Universalis = _universalis,
            GameUiManager = _gameUiManager,
            MarketCache = _marketCache,
            ChatUtilities =  _chatUtilities,
            GameInterface = _gameInterface,
            FileDialogManager = _fileDialogManager,
            MobTracker = _mockMobTracker,
            TooltipService = _tooltipService,
            HotkeyService = _hotkeyService,
            InventoryHistory = _inventoryHistory,
            TeleporterIpc = mockTeleporter,
            Font = _font
        });
        ConfigurationManager.Load(configFile);
        var inventories = ConfigurationManager.LoadInventory();
        PluginService.CharacterMonitor.LoadExistingRetainers(ConfigurationManager.Config.GetSavedRetainers());
        PluginService.InventoryMonitor.LoadExistingData(inventories);

        PluginService.InventoryHistory.LoadExistingHistory(ConfigurationManager.LoadHistoryFromCsv(out _));
        PluginService.InitaliseExplicit(new MockServices()
        {
        });
        _filterService = new FilterService(_characterMonitor, _inventoryMonitor, _inventoryHistory);
        _windowService = new WindowService(_filterService, Service.Log);
        _overlayService = new OverlayService(_filterService, _gameUiManager, Service.Framework);
        PluginService.InitaliseExplicit(new MockServices()
        {
            FilterService = _filterService,
            WindowService = _windowService,
            OverlayService = _overlayService,
        });
        _pluginLogic = new PluginLogic();
        PluginService.InitaliseExplicit(new MockServices()
        {
            PluginLogic = _pluginLogic,
        });
        PluginService.PluginLogic.RunMigrations();
            
        if (ConfigurationManager.Config.FirstRun)
        {
            PluginService.PluginLogic.LoadDefaultData();
            ConfigurationManager.Config.FirstRun = false;
        }
        
        PluginService.MarkLoaded();

        stopWatch.Stop();
        Service.Log.Verbose("Allagan Tools has finished loading. Total load time was " + stopWatch.Elapsed.TotalSeconds + " seconds.");
        var mockGameGuiWindow = new MockGameGuiWindow(mockService.MockGameGui, "Mock Game Gui");
        mockGameGuiWindow.IsOpen = true;
        _windowService.WindowSystem.AddWindow(mockGameGuiWindow);
        _windowService.OpenWindow<MockWindow>(MockWindow.AsKey);
        _isStarted = true;
    }

    public void Stop(MockProgram program, MockService mockService, MockPluginInterfaceService mockPluginInterfaceService)
    {
        _isStarted = false;
        Dispose();
    }
}
