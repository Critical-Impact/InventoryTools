using System;
using CriticalCommonLib;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public partial class InventoryToolsUi : IDisposable
    {
        private bool _disposing = false;
        
        
        public InventoryToolsUi()
        {
            Service.Interface.Draw += Draw;
            Service.Interface.OpenConfigUi += UiBuilderOnOpenConfigUi;
        }


        public InventoryToolsConfiguration Configuration
        {
            get
            {
                return ConfigurationManager.Config;
            }
        }

        private void UiBuilderOnOpenConfigUi()
        {
            PluginService.WindowService.ToggleConfigurationWindow();
        }

        public bool IsVisible
        {
            get => Configuration.IsVisible;
            set => Configuration.IsVisible = value;
        }

        public void Draw()
        {
            if (!PluginService.CharacterMonitor.IsLoggedIn || _disposing || !Service.ExcelCache.FinishedLoading || !PluginService.PluginLoaded)
                return;
            PluginService.WindowService.WindowSystem.Draw();

            PluginService.FileDialogManager.Draw();
        }
                        
        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                Service.Interface.Draw -= Draw;
                Service.Interface.OpenConfigUi -= UiBuilderOnOpenConfigUi;
            }
            _disposed = true;         
        }
    }
}