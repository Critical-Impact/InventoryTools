using System;
using CriticalCommonLib;
using Dalamud.Plugin;

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