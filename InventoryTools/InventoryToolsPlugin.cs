using System;
using System.Collections.Generic;
using System.IO;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
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
using FFXIVClientInterface;
using InventoryTools.Structs;

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
        internal ClientInterface ClientInterface { get; private set; }
        internal InventoryMonitor InventoryMonitor { get; private set; }
        internal NetworkMonitor NetworkMonitor { get; private set; }
        internal CharacterMonitor CharacterMonitor { get; private set; }
        internal PluginLogic PluginLogic { get; private set; }
        internal GameUi GameUi { get; private set; }
        
        public InventoryToolsConfiguration Config => _config;

        public InventoryToolsPlugin(DalamudPluginInterface pluginInterface, DataManager dataManager, SigScanner sigScanner, ClientState clientState, GameNetwork gameNetwork, Framework framework, CommandManager commandManager, ChatGui chatGui)
        {
            PluginInterface = pluginInterface;
            _config = (InventoryToolsConfiguration) this.PluginInterface.GetPluginConfig() ?? new InventoryToolsConfiguration();
            Config.Initialize(this.PluginInterface);
            _commandManager = new PluginCommandManager<InventoryToolsPlugin>(this, commandManager);

            ExcelCache.Initialise(dataManager);
            GameInterface = new GameInterface(sigScanner);
            OdrScanner = new OdrScanner(clientState);
            ClientInterface = new ClientInterface(sigScanner, dataManager);
            NetworkMonitor = new NetworkMonitor(gameNetwork);
            CharacterMonitor = new CharacterMonitor(gameNetwork,ClientInterface, framework, clientState);
            GameUi = new GameUi(sigScanner, framework);
            InventoryMonitor = new InventoryMonitor(ClientInterface, clientState, OdrScanner, CharacterMonitor, GameUi, gameNetwork);
            PluginLogic = new PluginLogic(Config, clientState, InventoryMonitor, CharacterMonitor, GameUi, chatGui);
            _ui = new InventoryToolsUi(pluginInterface,PluginLogic, InventoryMonitor, CharacterMonitor, Config, clientState, GameUi);
        }

        [Command("/inventorytools")]
        [HelpMessage("Shows the inventory tools window.")]
        public void ShowHideInventoryToolsCommand(string command, string args)
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
            ClientInterface.Dispose();
            OdrScanner.Dispose();
            Config.Save();
            ExcelCache.Destroy();
            PluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}