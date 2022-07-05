using System;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
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
            PluginService.FileDialogManager.Draw();
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
                        if (ImGui.BeginTabItem(itemTable.Name + "##" + filterConfiguration.Key))
                        {
                            ImGui.BeginChild("TopBar", new Vector2(0, 25)* ImGui.GetIO().FontGlobalScale);
                            var highlightItems = itemTable.HighlightItems;
                            ImGui.Checkbox( "Highlight?"+ "###" + itemTable.Key + "VisibilityCheckbox", ref highlightItems);
                            if (highlightItems != itemTable.HighlightItems)
                            {
                                PluginService.PluginLogic.ToggleActiveUiFilterByKey(itemTable.FilterConfiguration.Key);
                            }
                            if (ImGui.Button("Clear Search"))
                            {
                                //TODO: fix me bby
                            }
                            ImGui.EndChild();
                            ImGui.BeginChild("Content", new Vector2(0, -30)* ImGui.GetIO().FontGlobalScale); 
                            if (filterConfiguration.FilterType == FilterType.CraftFilter)
                            {
                                var craftTable = PluginService.PluginLogic.GetCraftTable(filterConfiguration.Key);
                                craftTable?.Draw(new Vector2(0, -400));
                                itemTable.Draw(new Vector2(0, 0));
                            }
                            else
                            {
                                itemTable.Draw(new Vector2(0, 0));
                            }
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
                            ImGui.EndChild();
                            //Need to have these buttons be determined dynamically or moved elsewhere
                            ImGui.BeginChild("BottomBar", new Vector2(0,0), false, ImGuiWindowFlags.None);
                            if (ImGui.Button("Refresh Market Prices"))
                            {
                                foreach (var item in itemTable.RenderSortedItems)
                                {
                                    Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                                }
                                foreach (var item in itemTable.RenderItems)
                                {
                                    Universalis.QueuePriceCheck(item.RowId);
                                }
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Export to CSV"))
                            {
                                //PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv", SaveCallback, null, true);
                            }
                            ImGui.SameLine();
                            if (filterConfiguration.FilterType == FilterType.CraftFilter)
                            {
                                unsafe
                                {
                                    var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
                                    if (subMarinePartsMenu != null)
                                    {
                                        if (ImGui.Button("Add Submarine Parts to Craft"))
                                        {
                                            var subAddon = (SubmarinePartsMenuAddon*)subMarinePartsMenu;
                                            for (int i = 0; i < 6; i++)
                                            {
                                                var itemRequired = subAddon->RequiredItemId(i);
                                                if (itemRequired != 0)
                                                {
                                                    var amountHandedIn = subAddon->AmountHandedIn(i);
                                                    var amountNeeded = subAddon->AmountNeeded(i);
                                                    var amountLeft = Math.Max((int)amountNeeded - (int)amountHandedIn, 0);
                                                    if (amountLeft > 0)
                                                    {
                                                        filterConfiguration.CraftList.AddCraftItem(itemRequired, (uint)amountLeft, ItemFlags.None);
                                                        filterConfiguration.NeedsRefresh = true;
                                                        filterConfiguration.StartRefresh();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            ImGui.SameLine();
                            ImGui.Text("Pending Market Requests: " + Universalis.QueuedCount);
                            if (filterConfiguration.FilterType == FilterType.CraftFilter)
                            {
                                ImGui.SameLine();
                                ImGui.Text("Total Cost NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                                ImGui.SameLine();
                                ImGui.Text("Total Cost HQ: " + filterConfiguration.CraftList.MinimumHQCost);
                                
                            }
                            ImGui.EndChild();
                            ImGui.EndTabItem();
                        }
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