using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public class FiltersPage : IConfigPage
    {
        public string Name { get; } = "Filters";
        public void Draw()
        {
            var filterConfigurations = PluginService.FilterService.FiltersList.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
            if (ImGui.CollapsingHeader("Filters", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("FilterConfigTable", 4, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                    ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("Order", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                    ImGui.TableHeadersRow();
                    if (filterConfigurations.Count == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("No filters available.");
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                    }

                    for (var index = 0; index < filterConfigurations.Count; index++)
                    {
                        ImGui.TableNextRow();
                        var filterConfiguration = filterConfigurations[index];
                        ImGui.TableNextColumn();
                        if (filterConfiguration.Name != "")
                        {
                            ImGui.Text(filterConfiguration.Name);
                            ImGui.SameLine();
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text(filterConfiguration.FormattedFilterType);

                        ImGui.TableNextColumn();
                        ImGui.Text((filterConfiguration.Order + 1).ToString());
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Up##" + index))
                        {
                            PluginService.FilterService.MoveFilterUp(filterConfiguration);
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Down##" + index))
                        {
                            PluginService.FilterService.MoveFilterDown(filterConfiguration);
                        }
                        
                        ImGui.TableNextColumn();
                        if (ImGui.SmallButton("Export Configuration##" + index))
                        {
                            var base64 = filterConfiguration.ExportBase64();
                            ImGui.SetClipboardText(base64);
                            PluginService.ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                        }

                        ImGui.SameLine();
                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            ImGui.OpenPopup("Delete?##" + index);
                        }

                        ImGui.SameLine();
                        if (ImGui.SmallButton("Open as Window##" + index))
                        {
                            PluginService.WindowService.OpenFilterWindow(filterConfiguration.Key);
                        }

                        if (ImGui.BeginPopupModal("Delete?##" + index))
                        {
                            ImGui.Text(
                                "Are you sure you want to delete this filter?.\nThis operation cannot be undone!\n\n");
                            ImGui.Separator();

                            if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                            {
                                PluginService.FilterService.RemoveFilter(filterConfiguration);
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.SetItemDefaultFocus();
                            ImGui.SameLine();
                            if (ImGui.Button("Cancel", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                            {
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.EndPopup();
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.PopStyleVar();
            }

            if (ImGui.CollapsingHeader("Create Filters", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (ImGui.Button("Add Search Filter"))
                {
                    PluginService.FilterService.AddFilter(new FilterConfiguration("New Search Filter",
                        Guid.NewGuid().ToString("N"), FilterType.SearchFilter));
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will create a new filter that let's you search for specific items within your characters and retainers inventories.");

                if (ImGui.Button("Add Sort Filter"))
                {
                    PluginService.FilterService.AddFilter(new FilterConfiguration("New Sort Filter",
                        Guid.NewGuid().ToString("N"), FilterType.SortingFilter));
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will create a new filter that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to.");


                if (ImGui.Button("Add Game Item Filter"))
                {
                    PluginService.FilterService.AddFilter(new FilterConfiguration("New Game Item Filter",
                        Guid.NewGuid().ToString("N"), FilterType.GameItemFilter));
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will create a filter that lets you search for all items in the game.");
            }

            if (ImGui.CollapsingHeader("Sample Filters", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.Text("Sample Filters:");
                if (ImGui.Button("Items that can be bought for 100 gil or less +"))
                {
                    PluginService.PluginLogic.AddSampleFilter100Gil();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will add a filter that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories.");

                if (ImGui.Button("Put away materials +"))
                {
                    PluginService.PluginLogic.AddSampleFilterMaterials();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will add a filter that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items.");

                if (ImGui.Button("Duplicated items across characters/retainers +"))
                {
                    PluginService.PluginLogic.AddSampleFilterDuplicatedItems();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will add a filter that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.");
                
                ImGui.Text("Default Filters:");
                if (ImGui.Button("All"))
                {
                    PluginService.PluginLogic.AddAllFilter();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This adds a new copy of the default 'All' filter.");
                if (ImGui.Button("Retainers"))
                {
                    PluginService.PluginLogic.AddRetainerFilter();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This adds a new copy of the default 'Retainer' filter.");
                
                if (ImGui.Button("Player"))
                {
                    PluginService.PluginLogic.AddPlayerFilter();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This adds a new copy of the default 'Player' filter.");

                if (ImGui.Button("All Game Items"))
                {
                    PluginService.PluginLogic.AddAllGameItemsFilter();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This adds a new copy of the default 'All Game Items' filter.");
            }
        }
    }
}