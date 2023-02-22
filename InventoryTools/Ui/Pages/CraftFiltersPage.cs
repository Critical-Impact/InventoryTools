using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public class CraftFiltersPage : IConfigPage
    {
        public string Name { get; } = "Craft Lists";
        public void Draw()
        {
            var filterConfigurations = PluginService.FilterService.FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            if (ImGui.CollapsingHeader("Filters", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("FilterConfigTable", 3, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                    ImGui.TableSetupColumn("Order", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                    ImGui.TableHeadersRow();
                    if (filterConfigurations.Count == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("No lists available.");
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
                if (ImGui.Button("Add New Craft List"))
                {
                    PluginService.FilterService.AddNewCraftFilter();
                }

                ImGui.SameLine();
                UiHelpers.HelpMarker(
                    "This will create a new list that can be accessed from within the craft window showing you a breakdown of the required materials.");
            }

        }
    }
}