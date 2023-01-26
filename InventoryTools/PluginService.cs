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
    public static class PluginService
    {
        public static InventoryMonitor InventoryMonitor { get; private set; } = null!;
        public static InventoryScanner InventoryScanner { get; private set; } = null!;
        public static CharacterMonitor CharacterMonitor { get; private set; } = null!;
        public static PluginLogic PluginLogic { get; private set; } = null!;
        public static GameUiManager GameUi { get; private set; } = null!;
        public static TryOn TryOn { get; private set; } = null!;
        public static PluginCommands PluginCommands { get; private set; } = null!;
        public static PluginCommandManager<PluginCommands> CommandManager { get; private set; } = null!;
        public static FilterService FilterService { get; private set; } = null!;
        public static OverlayService OverlayService { get; private set; } = null!;
        public static WindowService WindowService { get; private set; } = null!;
        public static WotsitIpc WotsitIpc { get; private set; } = null!;
        public static FileDialogManager FileDialogManager { get; private set; } = null!;
        public static CraftMonitor CraftMonitor { get; private set; } = null!;
        public static IconStorage IconStorage { get; private set; } = null!;
        public static ContextMenuService ContextMenuService { get; private set; } = null!;
        public static DalamudPluginInterface? PluginInterface { get; private set; } = null!;
        public static MarketCache MarketCache { get; private set; } = null!;
        public static Universalis Universalis { get; private set; } = null!;
        public static GameInterface GameInterface { get; private set; } = null!;
        
        public static IPCService IPCService { get; private set; } = null!;
        
        public static OdrScanner OdrScanner { get; private set; } = null!;
        public static bool PluginLoaded { get; private set; } = false;

        public delegate void PluginLoadedDelegate();
        public static event PluginLoadedDelegate? OnPluginLoaded; 

        public static void Initialise(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            Service.ExcelCache = new ExcelCache(Service.Data);
            ConfigurationManager.Load();
            Universalis = new Universalis();
            GameInterface = new GameInterface();
            MarketCache = new MarketCache(Universalis,Service.Interface.ConfigDirectory.FullName + "/universalis.json");
            
            CharacterMonitor = new CharacterMonitor();
            GameUi = new GameUiManager();
            TryOn = new TryOn();
            CraftMonitor = new CraftMonitor(GameUi);
            OdrScanner = new OdrScanner(CharacterMonitor);
            InventoryScanner = new InventoryScanner(CharacterMonitor, GameUi, GameInterface, OdrScanner);
            InventoryMonitor = new InventoryMonitor( CharacterMonitor,  CraftMonitor, InventoryScanner);
            InventoryScanner.Enable();
            
            FilterService = new FilterService( CharacterMonitor, InventoryMonitor);
            OverlayService = new OverlayService(FilterService, GameUi);
            ContextMenuService = new ContextMenuService();
            IconStorage = new IconStorage(Service.Interface, Service.Data);
            WindowService = new WindowService(FilterService);
            PluginLogic = new PluginLogic(  );
            WotsitIpc = new WotsitIpc(  );
            PluginCommands = new();
            CommandManager = new PluginCommandManager<PluginCommands>(PluginCommands);
            FileDialogManager = new FileDialogManager();
            ConfigurationManager.Config.RestoreServiceSettings();
            IPCService = new IPCService(pluginInterface, CharacterMonitor, FilterService, InventoryMonitor);
            PluginLoaded = true;
            OnPluginLoaded?.Invoke();
        }

        public static void InitialiseTesting(CharacterMonitor characterMonitor, PluginLogic pluginLogic)
        {
            CharacterMonitor = characterMonitor;
            PluginLogic = pluginLogic;
            PluginLoaded = true;
            OnPluginLoaded?.Invoke();
        }

        public static void Dispose()
        {
            PluginLoaded = false;
            IPCService.Dispose();
            ConfigurationManager.ClearQueue();
            ConfigurationManager.Save();
            ContextMenuService.Dispose();
            IconStorage.Dispose();
            CommandManager.Dispose();
            WotsitIpc.Dispose();
            PluginLogic.Dispose();
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
            PluginInterface = null!;
            MarketCache = null!;
            Universalis = null!;
            GameInterface = null!;
        }
    }
}