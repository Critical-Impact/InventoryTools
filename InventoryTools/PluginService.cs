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
        private static OdrScanner OdrScanner { get; set; } = null!;
        public static InventoryMonitor InventoryMonitor { get; private set; } = null!;
        private static NetworkMonitor NetworkMonitor { get; set; } = null!;
        public static CharacterMonitor CharacterMonitor { get; private set; } = null!;
        public static PluginLogic PluginLogic { get; private set; } = null!;
        public static GameUiManager GameUi { get; private set; } = null!;
        public static TryOn TryOn { get; private set; } = null!;
        //public static PluginFont PluginFont { get; private set; } = null!;
        public static PluginCommands PluginCommands { get; private set; } = null!;
        public static PluginCommandManager<PluginCommands> CommandManager { get; private set; } = null!;
        public static FilterService FilterService { get; private set; } = null!;
        public static OverlayService OverlayService { get; private set; } = null!;
        public static WindowService WindowService { get; private set; } = null!;
        public static WotsitIpc WotsitIpc { get; private set; } = null!;
        public static FileDialogManager FileDialogManager { get; private set; } = null!;
        public static CraftMonitor CraftMonitor { get; private set; } = null!;
        public static IconStorage IconStorage { get; private set; } = null!;
        public static DalamudContextMenu ContextMenu { get; private set; } = null!;
        public static DalamudPluginInterface? PluginInterface { get; private set; } = null!;
        public static bool PluginLoaded { get; private set; } = false;

        public delegate void PluginLoadedDelegate();
        public static event PluginLoadedDelegate? OnPluginLoaded; 

        public static void Initialise(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            Service.ExcelCache = new ExcelCache(Service.Data);
            ConfigurationManager.Load();
            Universalis.Initalise();
            GameInterface.Initialise(Service.Scanner);
            Cache.Initalise(Service.Interface.ConfigDirectory.FullName + "/universalis.json");
            
            NetworkMonitor = new NetworkMonitor();
            CharacterMonitor = new CharacterMonitor();
            OdrScanner = new OdrScanner( CharacterMonitor);
            GameUi = new GameUiManager();
            TryOn = new TryOn();
            CraftMonitor = new CraftMonitor(GameUi);
            InventoryMonitor = new InventoryMonitor(OdrScanner, CharacterMonitor, GameUi, CraftMonitor);
            
            FilterService = new FilterService( CharacterMonitor, InventoryMonitor);
            OverlayService = new OverlayService(FilterService, GameUi);
            ContextMenu = new DalamudContextMenu();
            IconStorage = new IconStorage(Service.Interface, Service.Data);
            WindowService = new WindowService(FilterService);
            PluginLogic = new PluginLogic(  );
            WotsitIpc = new WotsitIpc(  );
            PluginCommands = new();
            CommandManager = new PluginCommandManager<PluginCommands>(PluginCommands);
            FileDialogManager = new FileDialogManager();
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
            ContextMenu.Dispose();
            IconStorage.Dispose();
            CommandManager.Dispose();
            WotsitIpc.Dispose();
            PluginLogic.Dispose();
            FilterService.Dispose();
            OverlayService.Dispose();
            InventoryMonitor.Dispose();
            CraftMonitor.Dispose();
            TryOn.Dispose();
            GameUi.Dispose();
            CharacterMonitor.Dispose();
            NetworkMonitor.Dispose();
            OdrScanner.Dispose();
            ConfigurationManager.Save();
            Service.ExcelCache.Destroy();
            Cache.Dispose();
            Universalis.Dispose();
            //PluginFont?.Dispose();
            GameInterface.Dispose();
            if (TetrisGame.HasInstance)
            {
                TetrisGame.Instance.Dispose();
            }
            Cache.SaveCache(true);
            PluginInterface = null;
        }
    }
}