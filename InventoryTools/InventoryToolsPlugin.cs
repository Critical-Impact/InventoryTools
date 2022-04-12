using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Network;
using Dalamud.Logging;
using Dalamud.Plugin;
using DalamudPluginProjectTemplate.Attributes;
using FFXIVClientInterface;
using InventoryTools.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryTools
{
    public class InventoryToolsPlugin : IDalamudPlugin
    {
        private InventoryToolsUi _ui;
        public string Name => "Inventory Tools";
        internal DalamudPluginInterface PluginInterface { get; private set; }
        
        public InventoryToolsPlugin(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            PluginInterface.Create<Service>();
            PluginService.Initialise();
            _ui = new InventoryToolsUi();

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _ui.Dispose();
            PluginService.Dispose();            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}