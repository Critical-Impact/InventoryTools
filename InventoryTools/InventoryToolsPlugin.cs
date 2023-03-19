using System;
using CriticalCommonLib;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace InventoryTools
{
    public class InventoryToolsPlugin : IDalamudPlugin
    {
        private Ui.InventoryToolsUi _ui;
        public string Name => "Allagan Tools";
        internal DalamudPluginInterface PluginInterface { get; private set; }
        
        public InventoryToolsPlugin(DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            PluginInterface.Create<Service>();
            PluginService.Initialise(PluginInterface);
            _ui = new Ui.InventoryToolsUi();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            PluginLog.Verbose("Starting dispose of InventoryToolsPlugin");
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