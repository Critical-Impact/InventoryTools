using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.ClientState;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Sections;

namespace InventoryTools
{
    public partial class InventoryToolsUi : IDisposable
    {
        private bool _disposing = false;
        private string _activeFilter = "";
        
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
            Configuration.IsVisible = true;
        }

        public bool IsVisible
        {
            get => Configuration.IsVisible;
            set => Configuration.IsVisible = value;
        }
        
        public void Draw()
        {
            if (!Service.ClientState.IsLoggedIn || _disposing)
                return;
            PluginLogic.DrawCraftRequirementsWindow();
            PluginLogic.DrawFilterWindows();
            if (!IsVisible)
                return;
            var isVisible = IsVisible;
            ImGui.SetNextWindowSize(new Vector2(350, 350) * ImGui.GetIO().FontGlobalScale, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(350, 350) * ImGui.GetIO().FontGlobalScale, new Vector2(2000, 2000) * ImGui.GetIO().FontGlobalScale);
            ImGui.Begin("Inventory Tools", ref isVisible);
            if (ImGui.BeginTabBar("###InventoryTag", ImGuiTabBarFlags.FittingPolicyScroll))
            {
                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[index];
                    var itemTable = PluginService.PluginLogic.GetFilterTable(filterConfiguration.Key);
                    if (itemTable == null)
                    {
                        continue;
                    }
                    if (filterConfiguration.DisplayInTabs)
                    {
                        /*if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                        {
                            ImGui.PushFont(PluginFont.AppIcons.Value);
                            if (ImGui.BeginTabItem(filterConfiguration.Name + " " + filterConfiguration.Icon + "##" + filterConfiguration.Key))
                            {
                                ImGui.PopFont();
                                itemTable.Draw();
                                if (_activeFilter != filterConfiguration.Key)
                                {
                                    _activeFilter = filterConfiguration.Key;
                                    if (Configuration.SwitchFiltersAutomatically &&
                                        Configuration.ActiveUiFilter != filterConfiguration.Key &&
                                        Configuration.ActiveUiFilter != null)
                                    {
                                        PluginService.PluginLogic.ToggleActiveUiFilterByKey(filterConfiguration.Key);
                                    }
                                }

                                ImGui.EndTabItem();
                            }
                            else
                            {
                                ImGui.PopFont();
                            }
                        }
                        else
                        {*/
                            if (ImGui.BeginTabItem(itemTable.Name + "##" + filterConfiguration.Key))
                            {
                                itemTable.Draw();
                                if (_activeFilter != filterConfiguration.Key)
                                {
                                    _activeFilter = filterConfiguration.Key;
                                    if (Configuration.SwitchFiltersAutomatically &&
                                        Configuration.ActiveUiFilter != filterConfiguration.Key &&
                                        Configuration.ActiveUiFilter != null)
                                    {
                                        PluginService.PluginLogic.ToggleActiveUiFilterByKey(filterConfiguration.Key);
                                    }
                                }

                                ImGui.EndTabItem();
                            }

                        //}
                    }
                }
                
                if (Configuration.ShowFilterTab && ImGui.BeginTabItem("Filters"))
                {
                    FiltersSection.Draw();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Configuration"))
                {
                    ConfigurationSection.Draw();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Help"))
                {
                    DrawHelpTab();
                    ImGui.EndTabItem();
                }
                
                #if DEBUG
                if (ImGui.BeginTabItem("Debug"))
                {
                    DrawDebugUi();
                    ImGui.EndTabItem();
                }
                if (Configuration.TetrisEnabled && ImGui.BeginTabItem("Tetris"))
                {
                    DrawTetrisTab();
                    ImGui.EndTabItem();
                }
                #endif
                

                ImGui.EndTabBar();
            }
            ImGui.SameLine();
            ImGui.End();

            if (isVisible != IsVisible)
            {
                IsVisible = isVisible;
            }
        }

        public void Dispose()
        {
            _disposing = true;
            Service.Interface.UiBuilder.Draw -= Draw;
            Service.Interface.UiBuilder.OpenConfigUi -= UiBuilderOnOpenConfigUi;
        }
    }
}