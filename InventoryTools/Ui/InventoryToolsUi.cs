using System;
using CriticalCommonLib;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public partial class InventoryToolsUi : IDisposable
    {
        private bool _disposing = false;
        
        public InventoryToolsUi()
        {
            Service.Interface.UiBuilder.Draw += Draw;
            Service.Interface.UiBuilder.OpenConfigUi += UiBuilderOnOpenConfigUi;
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
            if (!Service.ClientState.IsLoggedIn || _disposing || !Service.ExcelCache.FinishedLoading || !PluginService.PluginLoaded)
                return;
            foreach (var window in PluginService.WindowService.Windows)
            {
                var windowVisible = window.Value.Visible;
                if (!windowVisible)
                {
                    continue;
                }
                ImGui.SetNextWindowSize(window.Value.Size * ImGui.GetIO().FontGlobalScale, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSizeConstraints(window.Value.MinSize * ImGui.GetIO().FontGlobalScale, window.Value.MaxSize * ImGui.GetIO().FontGlobalScale);
                if (window.Value.WindowFlags != null)
                {
                    ImGui.Begin(window.Value.Name, ref windowVisible, window.Value.WindowFlags.Value);
                }
                else
                {
                    ImGui.Begin(window.Value.Name, ref windowVisible);
                }
                window.Value.Draw();
                ImGui.End();
                if (windowVisible != window.Value.Visible)
                {
                    window.Value.Close();
                }
            }
            PluginService.FileDialogManager.Draw();
        }

        public void Dispose()
        {
            _disposing = true;
            Service.Interface.UiBuilder.Draw -= Draw;
            Service.Interface.UiBuilder.OpenConfigUi -= UiBuilderOnOpenConfigUi;
        }
    }
}