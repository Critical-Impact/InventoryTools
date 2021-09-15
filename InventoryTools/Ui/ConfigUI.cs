using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools
{
    public partial class InventoryToolsUi
    {
        private unsafe void DrawConfigurationTab()
        {
            var items = new string[] {"N/A", "Yes", "No"};

            if (ImGui.BeginChild("###ivConfigList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (ImGui.Selectable("General", _configuration.SelectedConfigurationPage == 0))
                {
                    _configuration.SelectedConfigurationPage = 0;
                }

                if (ImGui.Selectable("Filters", _configuration.SelectedConfigurationPage == 1))
                {
                    _configuration.SelectedConfigurationPage = 1;
                }

                for (var index = 0; index < _pluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = _pluginLogic.FilterConfigurations[index];
                    if (ImGui.Selectable("  - " + filterConfiguration.Name,
                        index + 2 == _configuration.SelectedConfigurationPage))
                    {
                        _configuration.SelectedConfigurationPage = index + 2;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivConfigView", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (_configuration.SelectedConfigurationPage == 0)
                {
                    var activeUiFilter = _pluginLogic.GetActiveUiFilter();
                    var activeBackgroundFilter = _pluginLogic.GetActiveBackgroundFilter();
                    ImGui.Text("Filter Details:");
                    ImGui.Separator();
                    ImGui.Text("Window Filter: " + (activeUiFilter != null ? activeUiFilter.Name : "Not Set"));
                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This is the filter that is active when the Inventory Tools window is visible.");
                    ImGui.Text("Background Filter: " +
                               (activeBackgroundFilter != null ? activeBackgroundFilter.Name : "Not Set"));
                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This is the filter that is active when the Inventory Tools window is not visible.");
                    ImGui.Separator();
                    ImGui.Text("General Options:");
                    ImGui.Separator();
                    bool showMonitorTab = _configuration.ShowFilterTab;
                    bool switchFiltersAutomatically = _configuration.SwitchFiltersAutomatically;
                    bool restorePreviousFilter = _configuration.RestorePreviousFilter;
                    
                    if (ImGui.Checkbox("Show Filters Tab?", ref showMonitorTab))
                    {
                        _configuration.ShowFilterTab = !_configuration.ShowFilterTab;
                    }

                    if (ImGui.Checkbox("Switch filters when navigating UI?", ref switchFiltersAutomatically))
                    {
                        _configuration.SwitchFiltersAutomatically = !_configuration.SwitchFiltersAutomatically;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the active window filter be switched automatically when switching tabs?.");

                    ImGui.Text("Retainer Visuals:");
                    ImGui.Separator();
                    if (ImGui.Checkbox("Color name in retainer list?", ref showMonitorTab))
                    {
                        _configuration.ShowFilterTab = !_configuration.ShowFilterTab;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the name of the retainer in the summoning bell list be coloured if a relevant item is to be sorted or is available in their inventory?");
                    if (ImGui.Checkbox("Show item number in retainer list?", ref showMonitorTab))
                    {
                        _configuration.ShowFilterTab = !_configuration.ShowFilterTab;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "Should the name of the retainer in the summoning bell list have the number of items to be sorted or are available in their inventory?");
                    ImGui.Text("Advanced Settings:");
                    ImGui.Separator();
                    if (ImGui.Checkbox("Allow Cross-Character Inventories?", ref showMonitorTab))
                    {
                        _configuration.ShowFilterTab = !_configuration.ShowFilterTab;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This is an experimental feature, should characters not currently logged in and their associated retainers be shown in filter configurations?");
                }
                else if (_configuration.SelectedConfigurationPage == 1)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5));
                    if (ImGui.BeginTable("FilterConfigTable", 3,  ImGuiTableFlags.BordersV |
                                                                 ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                                 ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                                 ImGuiTableFlags.BordersInnerH))
                    {
                        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f,(uint)0);
                        ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f,(uint)1);
                        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f,(uint)2);
                        ImGui.TableHeadersRow();
                        for (var index = 0; index < _pluginLogic.FilterConfigurations.Count; index++)
                        {
                            ImGui.TableNextRow();
                            var filterConfiguration = _pluginLogic.FilterConfigurations[index];
                            ImGui.TableNextColumn();
                            ImGui.Text(filterConfiguration.Name);
                            ImGui.TableNextColumn();
                            ImGui.Text(filterConfiguration.FormattedFilterType);
                            ImGui.TableNextColumn();
                            if (ImGui.Button("Remove##" + index))
                            {
                                ImGui.OpenPopup("Delete?##" + index);
                            }
                            if (ImGui.BeginPopupModal("Delete?##" + index))
                            {
                                ImGui.Text(
                                    "Are you sure you want to delete this filter?.\nThis operation cannot be undone!\n\n");
                                ImGui.Separator();

                                if (ImGui.Button("OK", new Vector2(120, 0)))
                                {
                                    _pluginLogic.RemoveFilter(filterConfiguration);
                                    ImGui.CloseCurrentPopup();
                                }

                                ImGui.SetItemDefaultFocus();
                                ImGui.SameLine();
                                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                                {
                                    ImGui.CloseCurrentPopup();
                                }

                                ImGui.EndPopup();
                            }
                        }
                        ImGui.EndTable();
                    }
                    ImGui.PopStyleVar();



                    
                    if (ImGui.Button("Add Sort Filter"))
                    {
                        _pluginLogic.FilterConfigurations.Add(new FilterConfiguration("New Filter",
                            Guid.NewGuid().ToString("N"), FilterType.SortingFilter));
                    }
                    
                    ImGui.SameLine();

                    if (ImGui.Button("Add Search Filter"))
                    {
                        _pluginLogic.FilterConfigurations.Add(new FilterConfiguration("New Filter",
                            Guid.NewGuid().ToString("N"), FilterType.SearchFilter));
                    }

                }
                if (_configuration.SelectedConfigurationPage > 1)
                {
                    var selectedFilter = _configuration.SelectedConfigurationPage - 2;
                    if (_pluginLogic.FilterConfigurations.Count > selectedFilter)
                    {
                        var filterConfiguration = _pluginLogic.FilterConfigurations[selectedFilter];
                        var filterName = filterConfiguration.Name;
                        var labelName = "##" + selectedFilter.ToString();
                        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(100);
                            ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
                            ImGui.SameLine();
                            ImGui.InputText(labelName + "FilterName", ref filterName, 100);
                            if (filterName != filterConfiguration.Name)
                            {
                                filterConfiguration.Name = filterName;
                            }

                            var filterType = filterConfiguration.FilterType == FilterType.SearchFilter
                                ? "Search"
                                : "Sort";
                            ImGui.SetNextItemWidth(100);
                            ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
                            ImGui.SameLine();
                            ImGui.TextDisabled(filterType);

                            ImGui.SetNextItemWidth(150);
                            ImGui.LabelText(labelName + "DisplayInTabs", "Display in Tab List?: ");
                            ImGui.SameLine();
                            var displayInTabs = filterConfiguration.DisplayInTabs;
                            if (ImGui.Checkbox(labelName + "DisplayInTabsCheckbox", ref displayInTabs))
                            {
                                if (displayInTabs != filterConfiguration.DisplayInTabs)
                                {
                                    filterConfiguration.DisplayInTabs = displayInTabs;
                                }
                            }


                        }

                        ImGui.NewLine();
                        if (ImGui.CollapsingHeader("Inventories", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            var playerCharacters = _characterMonitor.GetPlayerCharacters();
                            var allCharacters = _characterMonitor.AllCharacters();
                            
                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "SourceAllRetainers", "Source from all Retainers: ");
                            ImGui.SameLine();
                            var sourceAllRetainers = filterConfiguration.SourceAllRetainers == null
                                ? 0
                                : (filterConfiguration.SourceAllRetainers.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "SourceAllRetainersCombo", ref sourceAllRetainers,
                                items, 3))
                            {
                                if (sourceAllRetainers == 0 &&
                                    filterConfiguration.SourceAllRetainers != null)
                                {
                                    filterConfiguration.SourceAllRetainers = null;
                                }
                                else if (sourceAllRetainers == 1 &&
                                         filterConfiguration.SourceAllRetainers != true)
                                {
                                    filterConfiguration.SourceAllRetainers = true;
                                }
                                else if (sourceAllRetainers == 2 &&
                                         filterConfiguration.SourceAllRetainers != false)
                                {
                                    filterConfiguration.SourceAllRetainers = false;
                                }
                            }
                            
                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "SourceAllCharacters", "Source from all Characters: ");
                            ImGui.SameLine();
                            var sourceAllCharacters = filterConfiguration.SourceAllCharacters == null
                                ? 0
                                : (filterConfiguration.SourceAllCharacters.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "SourceAllCharactersCombo", ref sourceAllCharacters,
                                items, 3))
                            {
                                if (sourceAllCharacters == 0 &&
                                    filterConfiguration.SourceAllCharacters != null)
                                {
                                    filterConfiguration.SourceAllCharacters = null;
                                }
                                else if (sourceAllCharacters == 1 &&
                                         filterConfiguration.SourceAllCharacters != true)
                                {
                                    filterConfiguration.SourceAllCharacters = true;
                                }
                                else if (sourceAllCharacters == 2 &&
                                         filterConfiguration.SourceAllCharacters != false)
                                {
                                    filterConfiguration.SourceAllCharacters = false;
                                }
                            }
                            
                            ImGui.LabelText(labelName + "SourceLabel", "Source Inventories: ");

                            var currentSource = "";
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.BeginCombo(labelName + "SourceCombo", currentSource))
                            {
                                for (var playerIndex = 0; playerIndex < allCharacters.Length; playerIndex++)
                                {
                                    if (ImGui.Selectable(allCharacters[playerIndex].Value.Name,
                                        currentSource == allCharacters[playerIndex].Value.Name))
                                    {
                                        filterConfiguration.AddSourceInventory((allCharacters[playerIndex].Key,
                                            allCharacters[playerIndex].Value.OwnerId == 0
                                                ? InventoryCategory.CharacterBags
                                                : InventoryCategory.RetainerBags));
                                    }
                                }

                                ImGui.EndCombo();
                            }

                            for (var index = 0; index < filterConfiguration.SourceInventories.Count; index++)
                            {
                                var sourceInventory = filterConfiguration.SourceInventories[index];
                                if (index % 6 != 0 || index == 0)
                                {
                                    ImGui.SameLine();
                                }

                                if (ImGui.Button(_pluginLogic.GetCharacterName(sourceInventory.Item1) + " X"))
                                {
                                    filterConfiguration.RemoveSourceInventory(sourceInventory);
                                }
                            }

                            if (filterConfiguration.FilterType == FilterType.SortingFilter)
                            {
                                ImGui.SetNextItemWidth(200);
                                ImGui.LabelText(labelName + "DestinationAllRetainers", "Destination to all Retainers: ");
                                ImGui.SameLine();
                                var destinationAllRetainers = filterConfiguration.DestinationAllRetainers == null
                                    ? 0
                                    : (filterConfiguration.DestinationAllRetainers.Value ? 1 : 2);
                                if (ImGui.Combo(labelName + "DestinationAllRetainersCombo", ref destinationAllRetainers,
                                    items, 3))
                                {
                                    if (destinationAllRetainers == 0 &&
                                        filterConfiguration.DestinationAllRetainers != null)
                                    {
                                        filterConfiguration.DestinationAllRetainers = null;
                                    }
                                    else if (destinationAllRetainers == 1 &&
                                             filterConfiguration.DestinationAllRetainers != true)
                                    {
                                        filterConfiguration.DestinationAllRetainers = true;
                                    }
                                    else if (destinationAllRetainers == 2 &&
                                             filterConfiguration.DestinationAllRetainers != false)
                                    {
                                        filterConfiguration.DestinationAllRetainers = false;
                                    }
                                }
                                
                                ImGui.SetNextItemWidth(200);
                                ImGui.LabelText(labelName + "DestinationAllCharacters", "Destination to all Characters: ");
                                ImGui.SameLine();
                                var destinationAllCharacters = filterConfiguration.DestinationAllCharacters == null
                                    ? 0
                                    : (filterConfiguration.DestinationAllCharacters.Value ? 1 : 2);
                                if (ImGui.Combo(labelName + "DestinationAllCharactersCombo", ref destinationAllCharacters,
                                    items, 3))
                                {
                                    if (destinationAllCharacters == 0 &&
                                        filterConfiguration.DestinationAllCharacters != null)
                                    {
                                        filterConfiguration.DestinationAllCharacters = null;
                                    }
                                    else if (destinationAllCharacters == 1 &&
                                             filterConfiguration.DestinationAllCharacters != true)
                                    {
                                        filterConfiguration.DestinationAllCharacters = true;
                                    }
                                    else if (destinationAllCharacters == 2 &&
                                             filterConfiguration.DestinationAllCharacters != false)
                                    {
                                        filterConfiguration.DestinationAllCharacters = false;
                                    }
                                }
                                
                                ImGui.LabelText(labelName + "DestinationLabel", "Destination Inventories: ");

                                var currentDestination = "";
                                ImGui.SetNextItemWidth(100);
                                if (ImGui.BeginCombo(labelName + "DestinationCombo", currentDestination))
                                {
                                    for (var playerIndex = 0; playerIndex < allCharacters.Length; playerIndex++)
                                    {
                                        if (ImGui.Selectable(allCharacters[playerIndex].Value.Name,
                                            currentDestination == allCharacters[playerIndex].Value.Name))
                                        {
                                            filterConfiguration.AddDestinationInventory((allCharacters[playerIndex].Key,
                                                allCharacters[playerIndex].Value.OwnerId == 0
                                                    ? InventoryCategory.CharacterBags
                                                    : InventoryCategory.RetainerBags));
                                        }
                                    }

                                    ImGui.EndCombo();
                                }

                                for (var index = 0; index < filterConfiguration.DestinationInventories.Count; index++)
                                {
                                    var destinationInventory = filterConfiguration.DestinationInventories[index];
                                    if (index % 6 != 0 || index == 0)
                                    {
                                        ImGui.SameLine();
                                    }

                                    if (ImGui.Button(_pluginLogic.GetCharacterName(destinationInventory.Item1) + " X"))
                                    {
                                        filterConfiguration.RemoveDestinationInventory(destinationInventory);
                                    }
                                }
                            }
                        }


                        ImGui.NewLine();

                        if (ImGui.CollapsingHeader("Categories", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.LabelText(labelName + "UiCategoryLabel", "UI Categories: ");

                            var currentUiCategory = "";
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.BeginCombo(labelName + "UiCategoryCombo", currentUiCategory))
                            {
                                foreach (var item in ExcelCache.GetAllItemUICategories().Values
                                    .OrderBy(c => c.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-")))
                                {
                                    if (item.Name == "")
                                    {
                                        continue;
                                    }

                                    if (ImGui.Selectable(item.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-"), currentUiCategory == item.Name))
                                    {
                                        filterConfiguration.AddItemUiCategory(item.RowId);
                                    }
                                }

                                ImGui.EndCombo();
                            }

                            for (var index = 0; index < filterConfiguration.ItemUiCategoryId.Count; index++)
                            {
                                var itemUiCategoryId = filterConfiguration.ItemUiCategoryId[index];
                                var itemUiCategory = ExcelCache.GetItemUICategory(itemUiCategoryId);
                                if (itemUiCategory != null)
                                {
                                    var itemUiCategoryName = itemUiCategory.Name.ToString()
                                        .Replace("\u0002\u001F\u0001\u0003", "-");
                                    if (ImGui.Button(itemUiCategoryName + " X"))
                                    {
                                        filterConfiguration.RemoveItemUiCategory(itemUiCategoryId);
                                    }
                                }

                                if (index != filterConfiguration.ItemUiCategoryId.Count - 1 &&
                                    (index % 6 != 0 || index == 0))
                                {
                                    ImGui.SameLine();
                                }
                            }

                            ImGui.LabelText(labelName + "SearchCategoryLabel", "Search Categories: ");

                            var currentSearchCategory = "";
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.BeginCombo(labelName + "SearchCategoryCombo", currentSearchCategory))
                            {
                                foreach (var item in ExcelCache.GetAllItemSearchCategories().Values
                                    .OrderBy(c => c.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-")))
                                {
                                    if (item.Name == "")
                                    {
                                        continue;
                                    }

                                    if (ImGui.Selectable(item.Name.ToString().Replace("\u0002\u001F\u0001\u0003", "-"), currentSearchCategory == item.Name))
                                    {
                                        filterConfiguration.AddItemSearchCategory(item.RowId);
                                    }
                                }

                                ImGui.EndCombo();
                            }

                            for (var index = 0; index < filterConfiguration.ItemSearchCategoryId.Count; index++)
                            {
                                var itemSearchCategoryId = filterConfiguration.ItemSearchCategoryId[index];
                                var itemSearchCategory = ExcelCache.GetItemSearchCategory(itemSearchCategoryId);
                                if (itemSearchCategory != null)
                                {
                                    var itemSearchCategoryName = itemSearchCategory.Name.ToString()
                                        .Replace("\u0002\u001F\u0001\u0003", "-");
                                    if (ImGui.Button(itemSearchCategoryName + " X"))
                                    {
                                        filterConfiguration.RemoveItemSearchCategory(itemSearchCategoryId);
                                    }
                                }

                                if (index != filterConfiguration.ItemSearchCategoryId.Count - 1 &&
                                    (index % 6 != 0 || index == 0))
                                {
                                    ImGui.SameLine();
                                }
                            }
                        }

                        ImGui.NewLine();

                        if (ImGui.CollapsingHeader("Filters", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "IsHQLabel", "Is HQ?: ");
                            ImGui.SameLine();
                            var isHQ = filterConfiguration.IsHq == null ? 0 : (filterConfiguration.IsHq.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "IsHQCheckbox", ref isHQ, items, 3))
                            {
                                if (isHQ == 0 && filterConfiguration.IsHq != null)
                                {
                                    filterConfiguration.IsHq = null;
                                }
                                else if (isHQ == 1 && filterConfiguration.IsHq != true)
                                {
                                    filterConfiguration.IsHq = true;
                                }
                                else if (isHQ == 2 && filterConfiguration.IsHq != false)
                                {
                                    filterConfiguration.IsHq = false;
                                }
                            }

                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "IsCollectibleLabel", "Is Collectible?: ");
                            ImGui.SameLine();
                            var isCollectible = filterConfiguration.IsCollectible == null
                                ? 0
                                : (filterConfiguration.IsCollectible.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "IsCollectibleCheckbox", ref isCollectible, items, 3))
                            {
                                if (isCollectible == 0 && filterConfiguration.IsCollectible != null)
                                {
                                    filterConfiguration.IsCollectible = null;
                                }
                                else if (isCollectible == 1 && filterConfiguration.IsCollectible != true)
                                {
                                    filterConfiguration.IsCollectible = true;
                                }
                                else if (isCollectible == 2 && filterConfiguration.IsCollectible != false)
                                {
                                    filterConfiguration.IsCollectible = false;
                                }
                            }

                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "DuplicatesOnlyLabel", "Duplicates Only?: ");
                            ImGui.SameLine();
                            var duplicatesOnly = filterConfiguration.DuplicatesOnly == null
                                ? 0
                                : (filterConfiguration.DuplicatesOnly.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "DuplicatesOnlyCheckbox", ref duplicatesOnly, items, 3))
                            {
                                if (duplicatesOnly == 0 && filterConfiguration.DuplicatesOnly != null)
                                {
                                    filterConfiguration.DuplicatesOnly = null;
                                }
                                else if (duplicatesOnly == 1 && filterConfiguration.DuplicatesOnly != true)
                                {
                                    filterConfiguration.DuplicatesOnly = true;
                                }
                                else if (duplicatesOnly == 2 && filterConfiguration.DuplicatesOnly != false)
                                {
                                    filterConfiguration.DuplicatesOnly = false;
                                }
                            }

                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "ShowRelevantSourceOnly", "Show Relevant Source Only?: ");
                            ImGui.SameLine();
                            var ShowRelevantSourceOnly = filterConfiguration.ShowRelevantSourceOnly == null
                                ? 0
                                : (filterConfiguration.ShowRelevantSourceOnly.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "ShowRelevantSourceOnlyCheckbox", ref ShowRelevantSourceOnly,
                                items,
                                3))
                            {
                                if (ShowRelevantSourceOnly == 0 && filterConfiguration.ShowRelevantSourceOnly != null)
                                {
                                    filterConfiguration.ShowRelevantSourceOnly = null;
                                }
                                else if (ShowRelevantSourceOnly == 1 &&
                                         filterConfiguration.ShowRelevantSourceOnly != true)
                                {
                                    filterConfiguration.ShowRelevantSourceOnly = true;
                                }
                                else if (ShowRelevantSourceOnly == 2 &&
                                         filterConfiguration.ShowRelevantSourceOnly != false)
                                {
                                    filterConfiguration.ShowRelevantSourceOnly = false;
                                }
                            }

                            ImGui.SetNextItemWidth(200);
                            ImGui.LabelText(labelName + "ShowRelevantDestOnly", "Show Relevant Destination Only?: ");
                            ImGui.SameLine();
                            var ShowRelevantDestinationOnly = filterConfiguration.ShowRelevantDestinationOnly == null
                                ? 0
                                : (filterConfiguration.ShowRelevantDestinationOnly.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "ShowRelevantDestOnlyCheckbox", ref ShowRelevantDestinationOnly,
                                items, 3))
                            {
                                if (ShowRelevantDestinationOnly == 0 &&
                                    filterConfiguration.ShowRelevantDestinationOnly != null)
                                {
                                    filterConfiguration.ShowRelevantDestinationOnly = null;
                                }
                                else if (ShowRelevantDestinationOnly == 1 &&
                                         filterConfiguration.ShowRelevantDestinationOnly != true)
                                {
                                    filterConfiguration.ShowRelevantDestinationOnly = true;
                                }
                                else if (ShowRelevantDestinationOnly == 2 &&
                                         filterConfiguration.ShowRelevantDestinationOnly != false)
                                {
                                    filterConfiguration.ShowRelevantDestinationOnly = false;
                                }
                            }
                        }
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}