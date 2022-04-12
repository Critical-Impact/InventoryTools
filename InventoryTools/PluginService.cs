using CriticalCommonLib;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Plugin;
using FFXIVClientInterface;
using InventoryTools.Logic;

namespace InventoryTools
{
    public static class PluginService
    {
        private static OdrScanner OdrScanner { get; set; } = null!;
        public static ClientInterface ClientInterface { get; private set; } = null!;
        public static InventoryMonitor InventoryMonitor { get; private set; } = null!;
        private static NetworkMonitor NetworkMonitor { get; set; } = null!;
        public static CharacterMonitor CharacterMonitor { get; private set; } = null!;
        public static PluginLogic PluginLogic { get; private set; } = null!;
        public static GameUi GameUi { get; private set; } = null!;
        public static TryOn TryOn { get; private set; } = null!;
        public static PluginFont PluginFont { get; private set; } = null!;
        public static PluginCommands PluginCommands { get; private set; } = null!;
        public static PluginCommandManager<PluginCommands> CommandManager { get; private set; } = null!;

        public static void Initialise()
        {
            ConfigurationManager.Load();
            PluginFont = new PluginFont();
            ExcelCache.Initialise();
            Universalis.Initalise();
            GameInterface.Initialise(Service.Scanner);
            Cache.Initalise(Service.Interface.ConfigDirectory.FullName + "/universalis.json");
            ClientInterface = new ClientInterface(Service.Scanner, Service.Data);
            NetworkMonitor = new NetworkMonitor();
            CharacterMonitor = new CharacterMonitor(ClientInterface);
            OdrScanner = new OdrScanner( CharacterMonitor);
            GameUi = new GameUi();
            TryOn = new TryOn();
            InventoryMonitor = new InventoryMonitor(ClientInterface, OdrScanner, CharacterMonitor, GameUi);
            PluginLogic = new PluginLogic(  );
            PluginCommands = new();
            CommandManager = new PluginCommandManager<PluginCommands>(PluginCommands);

        }

        public static void Dispose()
        {
            CommandManager.Dispose();
            PluginLogic.Dispose();
            InventoryMonitor.Dispose();
            TryOn.Dispose();
            GameUi.Dispose();
            CharacterMonitor.Dispose();
            NetworkMonitor.Dispose();
            ClientInterface.Dispose();
            OdrScanner.Dispose();
            ConfigurationManager.Save();
            ExcelCache.Destroy();
            Cache.Dispose();
            Universalis.Dispose();
            PluginFont?.Dispose();
            Cache.SaveCache(true);
        }
    }
}