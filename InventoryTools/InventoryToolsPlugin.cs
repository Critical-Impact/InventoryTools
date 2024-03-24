using System;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Time;
using DalaMock.Shared.Classes;
using Dalamud.Plugin;

namespace InventoryTools
{
    public class InventoryToolsPlugin : IDalamudPlugin
    {
        private Ui.InventoryToolsUi _ui;
        internal DalamudPluginInterface PluginInterface { get; private set; }
        
        internal PluginLoader _pluginLoader { get; private set; }
        
        public InventoryToolsPlugin(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            var service = PluginInterface.Create<Service>()!;
            Service.Interface = new PluginInterfaceService(pluginInterface);
            Service.SeTime = new SeTime();
            _pluginLoader = new PluginLoader(new PluginInterfaceService(pluginInterface), service);
            _pluginLoader.Build();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            Service.Log.Debug("Starting dispose of InventoryToolsPlugin");
            _pluginLoader.Dispose();            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}