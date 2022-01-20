using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Game.ClientState;
using Dalamud.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using InventoryTools.Logic;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools
{
    public partial class InventoryToolsUi : IDisposable
    {
        private InventoryMonitor _inventoryMonitor;
        private CharacterMonitor _characterMonitor;
        private InventoryToolsConfiguration _configuration;
        private ClientState _clientState;
        private PluginLogic _pluginLogic;
        private DalamudPluginInterface _pluginInterface;
        private GameUi _gameUi;
        public InventoryToolsUi(DalamudPluginInterface pluginInterface, PluginLogic pluginLogic, InventoryMonitor inventoryMonitor, CharacterMonitor characterMonitor, InventoryToolsConfiguration configuration, ClientState clientState, GameUi gameUi)
        {
            _pluginLogic = pluginLogic;
            _inventoryMonitor = inventoryMonitor;
            _configuration = configuration;
            _clientState = clientState;
            _characterMonitor = characterMonitor;
            _pluginInterface = pluginInterface;
            _gameUi = gameUi;
            _pluginInterface.UiBuilder.Draw += Draw;
            _pluginInterface.UiBuilder.OpenConfigUi += UiBuilderOnOpenConfigUi;
        }

        private void UiBuilderOnOpenConfigUi()
        {
            _configuration.IsVisible = true;
        }

        public bool IsVisible
        {
            get => _configuration.IsVisible;
            set => _configuration.IsVisible = value;
        }
        
        private int _selectedFilterTab = 0;
        private bool _disposing = false;
        private string _activeFilter;
        public void Draw()
        {
            if (!IsVisible || !this._clientState.IsLoggedIn || _disposing)
                return;
            var isVisible = IsVisible;
            ImGui.SetNextWindowSize(new Vector2(350, 350) * ImGui.GetIO().FontGlobalScale, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(350, 350) * ImGui.GetIO().FontGlobalScale, new Vector2(2000, 2000) * ImGui.GetIO().FontGlobalScale);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, 0xFF000000);
            ImGui.Begin("Inventory Tools", ref isVisible);
            if (ImGui.BeginTabBar("###InventoryTag", ImGuiTabBarFlags.FittingPolicyScroll))
            {
                for (var index = 0; index < _pluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = _pluginLogic.FilterConfigurations[index];
                    var itemTable = _pluginLogic.GetFilterTable(filterConfiguration.Key);
                    if (filterConfiguration.DisplayInTabs)
                    {
                        if (ImGui.BeginTabItem(itemTable.Name + "##" + filterConfiguration.Key))
                        {
                            itemTable.Draw();
                            if (_activeFilter != filterConfiguration.Key)
                            {
                                _activeFilter = filterConfiguration.Key;
                                if (_configuration.SwitchFiltersAutomatically &&
                                    _configuration.ActiveUiFilter != filterConfiguration.Key && _configuration.ActiveUiFilter != null)
                                {
                                    _pluginLogic.ToggleActiveUiFilterByKey(filterConfiguration.Key);
                                }
                            }
                            ImGui.EndTabItem();
                        }
                    }
                }
                
                if (_configuration.ShowFilterTab && ImGui.BeginTabItem("Filters"))
                {
                    RenderMonitorTab();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Configuration"))
                {
                    DrawConfigurationTab();
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
                #endif

                ImGui.EndTabBar();
            }
            ImGui.SameLine();
            ImGui.End();

            if (isVisible != IsVisible)
            {
                IsVisible = isVisible;
            }
            ImGui.PopStyleColor();
        }

        private void RenderMonitorTab()
        {
            if (ImGui.BeginChild("###monitorLeft", new Vector2(100, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                for (var index = 0; index < _pluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = _pluginLogic.FilterConfigurations[index];
                    if (ImGui.Selectable(filterConfiguration.Name, index == _selectedFilterTab))
                    {
                        if (_configuration.SwitchFiltersAutomatically && _configuration.ActiveUiFilter != filterConfiguration.Key)
                        {
                            _pluginLogic.ToggleActiveBackgroundFilterByKey(filterConfiguration.Key);
                        }

                        _selectedFilterTab = index;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###monitorRight", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (var index = 0; index < _pluginLogic.FilterConfigurations.Count; index++)
                {
                    if (_selectedFilterTab == index)
                    {
                        var filterConfiguration = _pluginLogic.FilterConfigurations[index];
                        var table = _pluginLogic.GetFilterTable(filterConfiguration.Key);
                        if (table != null)
                        {
                            table.Draw();
                        }
                    }
                }
                ImGui.EndChild();
            }
        }
        
        public void Dispose()
        {
            _disposing = true;
            _pluginInterface.UiBuilder.Draw -= Draw;
            _pluginInterface.UiBuilder.OpenConfigUi -= UiBuilderOnOpenConfigUi;
        }
    }
}