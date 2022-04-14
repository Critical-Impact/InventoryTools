using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Configuration;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using DalamudPluginProjectTemplate.Attributes;
using InventoryTools.MarketBoard;
using InventoryTools.Resolvers;
using InventoryTools.Structs;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XivCommon;
using XivCommon.Functions.Tooltips;

namespace InventoryTools
{
    public class InventoryToolsPlugin : IDalamudPlugin
    {
        private PluginCommandManager<InventoryToolsPlugin> _commandManager;
        private InventoryToolsConfiguration _config;
        private InventoryToolsUi _ui;
        internal GameInterface GameInterface;
        public string Name => "Inventory Tools";
        internal DalamudPluginInterface PluginInterface { get; private set; }
        internal OdrScanner OdrScanner { get; private set; }
        internal InventoryMonitor InventoryMonitor { get; private set; }
        internal NetworkMonitor NetworkMonitor { get; private set; }
        internal CharacterMonitor CharacterMonitor { get; private set; }
        internal PluginLogic PluginLogic { get; private set; }
        internal GameUi GameUi { get; private set; }

        public InventoryToolsConfiguration Config => _config;
        
        public InventoryToolsConfiguration Load(DalamudPluginInterface pluginInterface)
        {
            if (!File.Exists(pluginInterface.ConfigFile.FullName))
                return null;
            return JsonConvert.DeserializeObject<InventoryToolsConfiguration>(File.ReadAllText(pluginInterface.ConfigFile.FullName), new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = new MinifyResolver()
            });
        }

        public InventoryToolsPlugin(DalamudPluginInterface pluginInterface, DataManager dataManager, SigScanner sigScanner, ClientState clientState, GameNetwork gameNetwork, Framework framework, CommandManager commandManager, ChatGui chatGui)
        {
            PluginInterface = pluginInterface;
            PluginInterface.Create<Service>();

            _config = this.Load(pluginInterface) ?? new InventoryToolsConfiguration();
            Config.Initialize(this.PluginInterface);
            _commandManager = new PluginCommandManager<InventoryToolsPlugin>(this, commandManager);

            ExcelCache.Initialise(dataManager);
            GameInterface = new GameInterface(sigScanner);
            NetworkMonitor = new NetworkMonitor(gameNetwork);
            CharacterMonitor = new CharacterMonitor(gameNetwork,framework, clientState);
            OdrScanner = new OdrScanner(clientState, CharacterMonitor);
            GameUi = new GameUi(sigScanner, framework);
            InventoryMonitor = new InventoryMonitor( clientState, OdrScanner, CharacterMonitor, GameUi, gameNetwork, framework);
            PluginLogic = new PluginLogic(Config, clientState, InventoryMonitor, CharacterMonitor, GameUi, chatGui, framework);
            _ui = new InventoryToolsUi(pluginInterface,PluginLogic, InventoryMonitor, CharacterMonitor, Config, clientState, GameUi);

            Cache.LoadCache(_config);
        }
        
        [Command("/inventorytools")]
        [HelpMessage("Shows the inventory tools window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
        {
            this.Config.IsVisible = !this.Config.IsVisible;
        }

        [Command("/inv")]
        [HelpMessage("Shows the inventory tools window.")]
        public void ShowHideInventoryToolsCommand2(string command, string args)
        {
            this.Config.IsVisible = !this.Config.IsVisible;
        }

        [Command("/invtools")]
        [HelpMessage("Shows the inventory tools window.")]
        public void ShowHideInventoryToolsCommand3(string command, string args)
        {
            this.Config.IsVisible = !this.Config.IsVisible;
        }

        [Command("/itfiltertoggle")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public void FilterToggleCommand(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }

        [Command("/invf")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public void FilterToggleCommand2(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }
        
        [Command("/ifilter")]
        [HelpMessage("Toggles the specified filter on/off and turns off any other filters.")]
        public void FilterToggleCommand3(string command, string args)
        {
            PluginLog.Verbose(command);
            PluginLog.Verbose(args);
            PluginLogic.ToggleActiveBackgroundFilterByName(args);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _ui.Dispose();
            _commandManager.Dispose();
            PluginLogic.Dispose();
            InventoryMonitor.Dispose();
            GameUi.Dispose();
            CharacterMonitor.Dispose();
            NetworkMonitor.Dispose();
            OdrScanner.Dispose();
            Config.Save();
            ExcelCache.Destroy();
            Universalis.Dispose();

            Cache.StoreCache();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}