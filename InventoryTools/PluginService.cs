using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.ContextMenu;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Plugin;
using InventoryTools.Commands;
using InventoryTools.Logic;
using InventoryTools.Misc;
using InventoryTools.Services;
using OtterGui.Classes;

namespace InventoryTools
{
    public struct MockServices
    {
        public IOverlayService? OverlayService;
        public IDataService? DataService;
        public IMarketCache? MarketCache;
        public IGameUiManager? GameUiManager;
        public WindowService? WindowService;
        public IUniversalis? Universalis;
        public IIconService? IconService;
        public IFrameworkService? FrameworkService;
        public IFilterService? FilterService;
        public IPluginInterfaceService? PluginInterfaceService;
        public PluginLogic? PluginLogic;
        public IInventoryMonitor? InventoryMonitor;
        public ICharacterMonitor? CharacterMonitor;
        public IInventoryScanner? InventoryScanner;
        public ICraftMonitor? CraftMonitor;
        public IChatUtilities? ChatUtilities;
        public IGameInterface? GameInterface;
    }

    public static class PluginService
    {
        public static IFrameworkService FrameworkService { get; private set; } = null!;
        public static ICommandService CommandService { get; private set; } = null!;
        public static IPluginInterfaceService PluginInterfaceService { get; private set; } = null!;
        public static IKeyStateService KeyStateService { get; private set; } = null!;
        public static IGuiService GuiService { get; private set; } = null!;
        public static IDataService DataService { get; private set; } = null!;
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
        public static TooltipService TooltipService { get; private set; } = null!;
        public static OdrScanner OdrScanner { get; private set; } = null!;
        public static bool PluginLoaded { get; private set; } = false;

        public delegate void PluginLoadedDelegate();
        public static event PluginLoadedDelegate? OnPluginLoaded; 

        public static void Initialise(DalamudPluginInterface pluginInterface)
        {
            Service.ExcelCache = new ExcelCache(Service.Data);
            FrameworkService = new FrameworkService(Service.Framework);
            CommandService = new CommandService(Service.Commands);
            PluginInterfaceService = new PluginInterfaceService(Service.Interface);
            KeyStateService = new KeyStateService(Service.KeyState);
            GuiService = new GuiService(Service.Gui);
            DataService = new DataService(Service.Data);
            ChatService = new ChatService(Service.Chat);
            ChatUtilities = new ChatUtilities();
            
            ConfigurationManager.Load();
            Universalis = new Universalis();
            GameInterface = new GameInterface();
            MarketCache = new MarketCache(Universalis,PluginService.PluginInterfaceService.ConfigDirectory.FullName + "/universalis.json");
            
            CharacterMonitor = new CharacterMonitor();
            GameUi = new GameUiManager();
            TryOn = new TryOn();
            CraftMonitor = new CraftMonitor(GameUi);
            OdrScanner = new OdrScanner(CharacterMonitor);
            InventoryScanner = new InventoryScanner(CharacterMonitor, GameUi, GameInterface, OdrScanner);
            InventoryMonitor = new InventoryMonitor( CharacterMonitor,  CraftMonitor, InventoryScanner, FrameworkService);
            InventoryScanner.Enable();
            
            FilterService = new FilterService( CharacterMonitor, InventoryMonitor);
            OverlayService = new OverlayService(FilterService, GameUi, FrameworkService);
            ContextMenuService = new ContextMenuService();
            IconStorage = new IconService(Service.Interface, Service.Data);
            WindowService = new WindowService(FilterService);
            TooltipService = new TooltipService();
            PluginLogic = new PluginLogic(  );
            WotsitIpc = new WotsitIpc(  );
            PluginCommands = new();
            CommandManager = new PluginCommandManager<PluginCommands>(PluginCommands);
            FileDialogManager = new FileDialogManager();
            ConfigurationManager.Config.RestoreServiceSettings();
            IpcService = new IPCService(pluginInterface, CharacterMonitor, FilterService, InventoryMonitor);
            PluginLoaded = true;
            OnPluginLoaded?.Invoke();
        }

        public static void InitaliseExplicit(MockServices mockServices, bool finishLoading = true)
        {
            if (mockServices.CharacterMonitor != null) CharacterMonitor = mockServices.CharacterMonitor;
            if (mockServices.InventoryMonitor != null) InventoryMonitor = mockServices.InventoryMonitor;
            if (mockServices.PluginLogic != null) PluginLogic = mockServices.PluginLogic;
            if (mockServices.FilterService != null) FilterService = mockServices.FilterService;
            if (mockServices.PluginInterfaceService != null) PluginInterfaceService = mockServices.PluginInterfaceService;
            if (mockServices.FrameworkService != null) FrameworkService = mockServices.FrameworkService;
            if (mockServices.IconService != null) IconStorage = mockServices.IconService;
            if (mockServices.Universalis != null) Universalis = mockServices.Universalis;
            if (mockServices.WindowService != null) WindowService = mockServices.WindowService;
            if (mockServices.GameUiManager != null) GameUi = mockServices.GameUiManager;
            if (mockServices.MarketCache != null) MarketCache = mockServices.MarketCache;
            if (mockServices.DataService != null) DataService = mockServices.DataService;
            if (mockServices.OverlayService != null) OverlayService = mockServices.OverlayService;
            if (mockServices.CraftMonitor != null) CraftMonitor = mockServices.CraftMonitor;
            if (mockServices.InventoryScanner != null) InventoryScanner = mockServices.InventoryScanner;
            if (mockServices.ChatUtilities != null) ChatUtilities = mockServices.ChatUtilities;
            if (mockServices.GameInterface != null) GameInterface = mockServices.GameInterface;
            PluginLoaded = true;
            if (finishLoading)
            {
                OnPluginLoaded?.Invoke();
            }
        }

        public static void Dispose()
        {
            PluginLoaded = false;
            IpcService.Dispose();
            ConfigurationManager.ClearQueue();
            ConfigurationManager.Save();
            ContextMenuService.Dispose();
            IconStorage.Dispose();
            CommandManager.Dispose();
            WotsitIpc.Dispose();
            PluginLogic.Dispose();
            TooltipService.Dispose();
            FilterService.Dispose();
            OverlayService.Dispose();
            InventoryMonitor.Dispose();
            InventoryScanner.Dispose();
            CraftMonitor.Dispose();
            TryOn.Dispose();
            GameUi.Dispose();
            CharacterMonitor.Dispose();
            Service.ExcelCache.Destroy();
            MarketCache.SaveCache(true);
            MarketCache.Dispose();
            Universalis.Dispose();
            GameInterface.Dispose();
            if (TetrisGame.HasInstance)
            {
                TetrisGame.Instance.Dispose();
            }
            FrameworkService.Dispose();
            PluginInterfaceService.Dispose();

            InventoryMonitor = null!;
            InventoryScanner = null!;
            CharacterMonitor = null!;
            PluginLogic = null!;
            GameUi = null!;
            TryOn = null!;
            TryOn = null!;
            PluginCommands = null!;
            CommandManager = null!;
            FilterService = null!;
            OverlayService = null!;
            WindowService = null!;
            WotsitIpc = null!;
            FileDialogManager = null!;
            CraftMonitor = null!;
            IconStorage = null!;
            ContextMenuService = null!;
            MarketCache = null!;
            Universalis = null!;
            GameInterface = null!;
            TooltipService = null!;
        }
    }
}