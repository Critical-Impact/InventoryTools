using System;
using CriticalCommonLib;
using DalaMock.Shared.Classes;
using Dalamud.Plugin;
using InventoryTools.Host;

namespace InventoryTools
{
    public class InventoryToolsPlugin : IDalamudPlugin
    {
        private Service? _service;
        private IDalamudPluginInterface? PluginInterface { get; set; }
        private PluginLoader? PluginLoader { get; set; }
        
        public InventoryToolsPlugin(IDalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            _service = PluginInterface.Create<Service>()!;
            PluginLoader = new PluginLoader(pluginInterface, _service);
            PluginLoader.Build();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            Service.Log.Debug("Starting dispose of InventoryToolsPlugin");
            PluginLoader?.Dispose();            
            _service?.Dispose();

            _service = null;
            PluginInterface = null;
            PluginLoader = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}