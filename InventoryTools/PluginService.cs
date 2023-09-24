using System.Diagnostics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Ipc;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Interfaces;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Commands;
using InventoryTools.Logic;
using InventoryTools.Misc;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;

namespace InventoryTools
{
    public struct MockServices
    {
        public IOverlayService? OverlayService;
        public IMarketCache? MarketCache;
        public IGameUiManager? GameUiManager;
        public WindowService? WindowService;
        public IUniversalis? Universalis;
        public IIconService? IconService;
        public IFrameworkService? FrameworkService;
        public IFilterService? FilterService;
        public PluginLogic? PluginLogic;
        public IInventoryMonitor? InventoryMonitor;
        public ICharacterMonitor? CharacterMonitor;
        public IInventoryScanner? InventoryScanner;
        public ICraftMonitor? CraftMonitor;
        public IChatUtilities? ChatUtilities;
        public IGameInterface? GameInterface;
        public FileDialogManager? FileDialogManager;
        public IMobTracker? MobTracker;
        public ITooltipService? TooltipService;
        public IHotkeyService? HotkeyService;
        public InventoryHistory? InventoryHistory;
        public ITeleporterIpc? TeleporterIpc;
    }

    public static class PluginService
    {
        public static IFrameworkService FrameworkService { get; private set; } = null!;
        public static IGuiService GuiService { get; private set; } = null!;
        public static IChatService ChatService { get; private set; } = null!;
        public static IInventoryMonitor InventoryMonitor { get; private set; } = null!;
        public static IInventoryScanner InventoryScanner { get; private set; } = null!;
        public static ICharacterMonitor CharacterMonitor { get; private set; } = null!;
        public static PluginLogic PluginLogic { get; private set; } = null!;
        public static IGameUiManager GameUi { get; private set; } = null!;
        public static TryOn TryOn { get; private set; } = null!;
        public static PluginCommands PluginCommands { get; private set; } = null!;
        public static PluginCommandManager<PluginCommands> CommandManager { get; private set; } = null!;
        public static IFilterService FilterService { get; private set; } = null!;
        public static IOverlayService OverlayService { get; private set; } = null!;
        public static WindowService WindowService { get; private set; } = null!;
        public static IWotsitIpc WotsitIpc { get; private set; } = null!;
        public static FileDialogManager FileDialogManager { get; private set; } = null!;
        public static ICraftMonitor CraftMonitor { get; private set; } = null!;
        public static IIconService IconStorage { get; private set; } = null!;
        public static ContextMenuService ContextMenuService { get; private set; } = null!;
        public static IMarketCache MarketCache { get; private set; } = null!;
        public static IUniversalis Universalis { get; private set; } = null!;
        public static IGameInterface GameInterface { get; private set; } = null!;
        public static IPCService IpcService { get; private set; } = null!;
        public static IChatUtilities ChatUtilities { get; private set; } = null!;
        public static ITooltipService TooltipService { get; private set; } = null!;
        public static OdrScanner OdrScanner { get; private set; } = null!;
        public static IMobTracker MobTracker { get; private set; } = null!;
        public static IHotkeyService HotkeyService { get; private set; } = null!;
        public static InventoryHistory InventoryHistory { get; private set; } = null!;
        public static ITeleporterIpc TeleporterIpc { get; private set; } = null!;
        public static bool PluginLoaded { get; private set; } = false;

        public delegate void PluginLoadedDelegate();
        public static event PluginLoadedDelegate? OnPluginLoaded; 

        public static void Initialise(DalamudPluginInterface pluginInterface)
        {
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            Service.SeTime = new SeTime();
            Service.ExcelCache = new ExcelCache(Service.Data);
            Service.ExcelCache.PreCacheItemData();
            FrameworkService = new FrameworkService(Service.Framework);
            GuiService = new GuiService(Service.GameGui);
            ChatService = new ChatService(Service.Chat);
            ChatUtilities = new ChatUtilities();
            MobTracker = new MobTracker(Service.GameInteropProvider);
            ConfigurationManager.Load();
            Universalis = new Universalis();
            GameInterface = new GameInterface(Service.GameInteropProvider);
            MarketCache = new MarketCache(Universalis,Service.Interface.ConfigDirectory.FullName + "/universalis.json");
            
            CharacterMonitor = new CharacterMonitor(Service.Framework, Service.ClientState, Service.ExcelCache);
            GameUi = new GameUiManager(Service.GameInteropProvider);
            TryOn = new TryOn();
            CraftMonitor = new CraftMonitor(GameUi);
            OdrScanner = new OdrScanner(CharacterMonitor);
            InventoryScanner = new InventoryScanner(CharacterMonitor, GameUi, GameInterface, OdrScanner,Service.GameInteropProvider);
            InventoryMonitor = new InventoryMonitor( CharacterMonitor,  CraftMonitor, InventoryScanner, FrameworkService);
            InventoryScanner.Enable();
            InventoryHistory = new InventoryHistory(InventoryMonitor);
            FilterService = new FilterService( CharacterMonitor, InventoryMonitor, InventoryHistory);
            OverlayService = new OverlayService(FilterService, GameUi, FrameworkService);
            ContextMenuService = new ContextMenuService();
            IconStorage = new IconService(Service.TextureProvider);
            WindowService = new WindowService(FilterService);
            TooltipService = new TooltipService(Service.GameInteropProvider);
            HotkeyService = new HotkeyService(FrameworkService, Service.KeyState);
            PluginLogic = new PluginLogic(  );
            WotsitIpc = new WotsitIpc(  );
            TeleporterIpc = new TeleporterIpc( pluginInterface );
            PluginCommands = new();
            CommandManager = new PluginCommandManager<PluginCommands>(PluginCommands, Service.Commands);
            FileDialogManager = new FileDialogManager();
            ConfigurationManager.Config.RestoreServiceSettings();
            IpcService = new IPCService(pluginInterface, CharacterMonitor, FilterService, InventoryMonitor);
            PluginLoaded = true;
            OnPluginLoaded?.Invoke();
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Allagan Tools has finished loading. Total load time was " + loadConfigStopwatch.Elapsed.TotalSeconds + " seconds.");
        }
        
        public static void InitaliseExplicit(MockServices mockServices, bool finishLoading = true)
        {
            if (mockServices.CharacterMonitor != null) CharacterMonitor = mockServices.CharacterMonitor;
            if (mockServices.InventoryMonitor != null) InventoryMonitor = mockServices.InventoryMonitor;
            if (mockServices.PluginLogic != null) PluginLogic = mockServices.PluginLogic;
            if (mockServices.FilterService != null) FilterService = mockServices.FilterService;
            if (mockServices.FrameworkService != null) FrameworkService = mockServices.FrameworkService;
            if (mockServices.IconService != null) IconStorage = mockServices.IconService;
            if (mockServices.Universalis != null) Universalis = mockServices.Universalis;
            if (mockServices.WindowService != null) WindowService = mockServices.WindowService;
            if (mockServices.GameUiManager != null) GameUi = mockServices.GameUiManager;
            if (mockServices.MarketCache != null) MarketCache = mockServices.MarketCache;
            if (mockServices.OverlayService != null) OverlayService = mockServices.OverlayService;
            if (mockServices.CraftMonitor != null) CraftMonitor = mockServices.CraftMonitor;
            if (mockServices.InventoryScanner != null) InventoryScanner = mockServices.InventoryScanner;
            if (mockServices.ChatUtilities != null) ChatUtilities = mockServices.ChatUtilities;
            if (mockServices.GameInterface != null) GameInterface = mockServices.GameInterface;
            if (mockServices.FileDialogManager != null) FileDialogManager = mockServices.FileDialogManager;
            if (mockServices.MobTracker != null) MobTracker = mockServices.MobTracker;
            if (mockServices.TooltipService != null) TooltipService = mockServices.TooltipService;
            if (mockServices.HotkeyService != null) HotkeyService = mockServices.HotkeyService;
            if (mockServices.InventoryHistory != null) InventoryHistory = mockServices.InventoryHistory;
            if (mockServices.TeleporterIpc != null) TeleporterIpc = mockServices.TeleporterIpc;
            if (finishLoading)
            {
                PluginLoaded = true;
                OnPluginLoaded?.Invoke();
            }
        }

        public static void Dispose()
        {
            PluginLoaded = false;
            HotkeyService.Dispose();
            TooltipService.Dispose();
            IpcService.Dispose();
            ContextMenuService.Dispose();
            IconStorage.Dispose();
            CommandManager.Dispose();
            WotsitIpc.Dispose();
            PluginLogic.Dispose();
            MobTracker.Dispose();
            TooltipService.Dispose();
            FilterService.Dispose();
            OverlayService.Dispose();
            InventoryHistory.Dispose();
            InventoryMonitor.Dispose();
            InventoryScanner.Dispose();
            WindowService.Dispose();
            CraftMonitor.Dispose();
            TryOn.Dispose();
            GameUi.Dispose();
            CharacterMonitor.Dispose();
            Service.ExcelCache.Destroy();
            Service.ExcelCache.Dispose();
            Service.SeTime.Dispose();
            MarketCache.SaveCache(true);
            MarketCache.Dispose();
            Universalis.Dispose();
            GameInterface.Dispose();
            OdrScanner.Dispose();
            CommandManager.Dispose();
            if (TetrisGame.HasInstance)
            {
                TetrisGame.Instance.Dispose();
            }
            ConfigurationManager.ClearQueue();
            ConfigurationManager.Save();
            ConfigurationManager.Dereference();
            FrameworkService.Dispose();

            Service.Dereference();

            Dereference();
        }

        public static void Dereference()
        {
            FrameworkService = null!;
            GuiService = null!;
            ChatService = null!;
            InventoryMonitor = null!;
            InventoryScanner = null!;
            InventoryHistory = null!;
            CharacterMonitor = null!;
            PluginLogic = null!;
            GameUi = null!;
            TryOn = null!;
            PluginCommands = null!;
            CommandManager = null!;
            FilterService = null!;
            OverlayService = null!;
            WindowService = null!;
            WotsitIpc = null!;
            TeleporterIpc = null!;
            FileDialogManager = null!;
            CraftMonitor = null!;
            IconStorage = null!;
            ContextMenuService = null!;
            MarketCache = null!;
            Universalis = null!;
            GameInterface = null!;
            IpcService = null!;
            ChatUtilities = null!;
            TooltipService = null!;
            OdrScanner = null!;
            MobTracker = null!;
            HotkeyService = null!;
            OnPluginLoaded = null;
        }
    }
}