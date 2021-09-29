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
                    if (ImGui.Selectable("  - " + filterConfiguration.Name + "##" + filterConfiguration.Key,
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
                    bool displayCrossCharacter = _configuration.DisplayCrossCharacter;
                    Vector3 highlightColor = _configuration.HighlightColor;
                    
                    if (ImGui.Checkbox("Show Filters Tab?", ref showMonitorTab))
                    {
                        _configuration.ShowFilterTab = !_configuration.ShowFilterTab;
                    }

                    if (ImGui.Checkbox("Switch filters when navigating UI?", ref switchFiltersAutomatically))
                    {
                        _configuration.SwitchFiltersAutomatically = !_configuration.SwitchFiltersAutomatically;
                    }

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
                    
                    if (ImGui.ColorEdit3("Highlight Color?", ref highlightColor, ImGuiColorEditFlags.NoInputs))
                    {
                        _configuration.HighlightColor = highlightColor;
                    }

                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "The color to set the highlighted items to.");
                    
                    ImGui.Text("Advanced Settings:");
                    ImGui.Separator();
                    if (ImGui.Checkbox("Allow Cross-Character Inventories?", ref displayCrossCharacter))
                    {
                        _configuration.DisplayCrossCharacter = !_configuration.DisplayCrossCharacter;
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
                    
                    ImGui.NewLine();
                    ImGui.Separator();
                    ImGui.Text("Sample Filters:");
                    if (ImGui.Button("Items that can be bought for 100 gil or less +"))
                    {
                        _pluginLogic.AddSampleFilter100Gil();
                    }                            
                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This will add a filter that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories.");
                    
                    if (ImGui.Button("Put away materials +"))
                    {
                        _pluginLogic.AddSampleFilterMaterials();
                    }
                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This will add a filter that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items.");
                    
                    if (ImGui.Button("Duplicated items across characters/retainers +"))
                    {
                        _pluginLogic.AddSampleFilterDuplicatedItems();
                    }
                    ImGui.SameLine();
                    UiHelpers.HelpMarker(
                        "This will add a filter that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.");

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
                            ImGui.LabelText(labelName + "DisplayInTabs", "Display in Tab List: ");
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

                        if (ImGui.CollapsingHeader("Inventories", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            var allCharacters = _characterMonitor.AllCharacters();
                            if (!_configuration.DisplayCrossCharacter)
                            {
                                allCharacters = allCharacters.Where(c =>
                                    PluginLogic.CharacterMonitor.BelongsToActiveCharacter(c.Key)).ToArray();
                            }
                            
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
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Use every retainer's inventory as a source.");
                            
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
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Use every characters's inventory as a source. This will generally only be your own character unless you have cross-character inventory tracking enabled.");
                            
                            ImGui.SetNextItemWidth(120);
                            ImGui.LabelText(labelName + "SourceLabel", "Source Inventories: ");
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "This is a list of source inventories to sort items from based on the filter configuration");

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
                                if (PluginLogic.CharacterMonitor.BelongsToActiveCharacter(sourceInventory.Item1) || _configuration.DisplayCrossCharacter)
                                {
                                    if (index % 6 != 0 || index == 0)
                                    {
                                        ImGui.SameLine();
                                    }

                                    if (ImGui.Button(_pluginLogic.GetCharacterName(sourceInventory.Item1) + " X"))
                                    {
                                        filterConfiguration.RemoveSourceInventory(sourceInventory);
                                    }
                                }
                            }

                            if (filterConfiguration.FilterType == FilterType.SortingFilter)
                            {
                                ImGui.Separator();
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
                                ImGui.SameLine();
                                UiHelpers.HelpMarker(
                                    "Use every retainer as a destination for items to be sorted.");
                                
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
                                ImGui.SameLine();
                                UiHelpers.HelpMarker(
                                    "Use every character as a destination for items to be sorted. This will generally only be your own character unless you have cross-character inventory tracking enabled.");
                                
                                ImGui.SetNextItemWidth(120);
                                ImGui.LabelText(labelName + "DestinationLabel", "Destination Inventories: ");
                                ImGui.SameLine();
                                UiHelpers.HelpMarker(
                                    "This is a list of destinations to sort items from source into based on the filter configuration.");

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
                                    if (PluginLogic.CharacterMonitor.BelongsToActiveCharacter(
                                        destinationInventory.Item1) || _configuration.DisplayCrossCharacter)
                                    {
                                        if (index % 6 != 0 || index == 0)
                                        {
                                            ImGui.SameLine();
                                        }
                                        if (ImGui.Button(_pluginLogic.GetCharacterName(destinationInventory.Item1) +
                                                         " X"))
                                        {
                                            filterConfiguration.RemoveDestinationInventory(destinationInventory);
                                        }
                                    }
                                }
                            }
                        }
                        if (ImGui.CollapsingHeader("Categories", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.LabelText(labelName + "UiCategoryLabel", "Categories: ");

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
                            
                            ImGui.Separator();

                            ImGui.LabelText(labelName + "SearchCategoryLabel", "Market Board Categories: ");

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

                        if (ImGui.CollapsingHeader("Filters", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "IsHQLabel", "Is HQ: ");
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
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Is the item High Quality?");

                            ImGui.SetNextItemWidth(205);
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
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Is the item Collectible?");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "Name", "Name: ");
                            ImGui.SameLine();
                            var name = filterConfiguration.NameFilter == null ? "" : filterConfiguration.NameFilter;
                            if (ImGui.InputText(labelName + "NameInput", ref name, 10))
                            {
                                if (name != filterConfiguration.NameFilter)
                                {
                                    filterConfiguration.NameFilter = name;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The name of the item. ! can be used for comparisons");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "Quantity", "Quantity: ");
                            ImGui.SameLine();
                            var quantity = filterConfiguration.Quantity == null ? "" : filterConfiguration.Quantity;
                            if (ImGui.InputText(labelName + "QuantityInput", ref quantity, 10))
                            {
                                if (quantity != filterConfiguration.Quantity)
                                {
                                    filterConfiguration.Quantity = quantity;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The quantity of the item. !,>,<,>=,<= can be used for comparisons");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "ILevel", "Item Level: ");
                            ImGui.SameLine();
                            var iLevel = filterConfiguration.iLevel == null ? "" : filterConfiguration.iLevel;
                            if (ImGui.InputText(labelName + "ILevelInput", ref iLevel, 10))
                            {
                                if (iLevel != filterConfiguration.iLevel)
                                {
                                    filterConfiguration.iLevel = iLevel;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The item level of the item. !,>,<,>=,<= can be used for comparisons");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "Spiritbond", "Spiritbond: ");
                            ImGui.SameLine();
                            var spiritbond = filterConfiguration.Spiritbond == null ? "" : filterConfiguration.Spiritbond;
                            if (ImGui.InputText(labelName + "SpiritbondInput", ref spiritbond, 10))
                            {
                                if (spiritbond != filterConfiguration.Spiritbond)
                                {
                                    filterConfiguration.Spiritbond = spiritbond;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The spiritbond percentage of the item. !,>,<,>=,<= can be used for comparisons");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "SellingPrice", "Gil Selling Price: ");
                            ImGui.SameLine();
                            var sellingPrice = filterConfiguration.ShopSellingPrice == null ? "" : filterConfiguration.ShopSellingPrice;
                            if (ImGui.InputText(labelName + "SellingPriceInput", ref sellingPrice, 10))
                            {
                                if (sellingPrice != filterConfiguration.ShopSellingPrice)
                                {
                                    filterConfiguration.ShopSellingPrice = sellingPrice;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The price when sold to the shops. !,>,<,>=,<= can be used for comparisons");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "BuyingPrice", "Gil Buying Price: ");
                            ImGui.SameLine();
                            var buyingPrice = filterConfiguration.ShopBuyingPrice == null ? "" : filterConfiguration.ShopBuyingPrice;
                            if (ImGui.InputText(labelName + "BuyingPriceInput", ref buyingPrice, 10))
                            {
                                if (buyingPrice != filterConfiguration.ShopBuyingPrice)
                                {
                                    filterConfiguration.ShopBuyingPrice = buyingPrice;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "The price when bought from shops. !,>,<,>=,<= can be used for comparisons");
                            
                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "CanBeBoughtLabel", "Can be Purchased for Gil: ");
                            ImGui.SameLine();
                            var canBeBought = filterConfiguration.CanBeBought == null
                                ? 0
                                : (filterConfiguration.CanBeBought.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "CanBeBoughtCheckbox", ref canBeBought, items, 3))
                            {
                                if (canBeBought == 0 && filterConfiguration.CanBeBought != null)
                                {
                                    filterConfiguration.CanBeBought = null;
                                }
                                else if (canBeBought == 1 && filterConfiguration.CanBeBought != true)
                                {
                                    filterConfiguration.CanBeBought = true;
                                }
                                else if (canBeBought == 2 && filterConfiguration.CanBeBought != false)
                                {
                                    filterConfiguration.CanBeBought = false;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Whether the item can be bought from a gil shop?");
                        }
                        if (ImGui.CollapsingHeader("Misc", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "DuplicatesOnlyLabel", "Duplicates Only: ");
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
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "Filter out any items that do not appear in both the source and destination?");

                            ImGui.SetNextItemWidth(205);
                            ImGui.LabelText(labelName + "FilterItemsInRetainers", "Filter Items when in Retainers: ");
                            ImGui.SameLine();
                            var FilterItemsInRetainers = filterConfiguration.FilterItemsInRetainers == null
                                ? 0
                                : (filterConfiguration.FilterItemsInRetainers.Value ? 1 : 2);
                            if (ImGui.Combo(labelName + "FilterItemsInRetainersCheckbox", ref FilterItemsInRetainers,
                                items, 3))
                            {
                                if (FilterItemsInRetainers == 0 &&
                                    filterConfiguration.FilterItemsInRetainers != null)
                                {
                                    filterConfiguration.FilterItemsInRetainers = null;
                                }
                                else if (FilterItemsInRetainers == 1 &&
                                         filterConfiguration.FilterItemsInRetainers != true)
                                {
                                    filterConfiguration.FilterItemsInRetainers = true;
                                }
                                else if (FilterItemsInRetainers == 2 &&
                                         filterConfiguration.FilterItemsInRetainers != false)
                                {
                                    filterConfiguration.FilterItemsInRetainers = false;
                                }
                            }
                            ImGui.SameLine();
                            UiHelpers.HelpMarker(
                                "When talking with a retainer should the filter adjust itself to only show items that should be put inside the retainer from your inventory?");
                        }
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}