using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public class ImportExportPage : IConfigPage
    {
        private bool _isSeparator = false;
        public string Name { get; } =  "Import/Export";
        public void Draw()
        {
            ImGui.PushID("ImportSection");
            if (ImGui.CollapsingHeader("Export", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var filterConfigurations = PluginService.FilterService.FiltersList;
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.BeginTable("FilterConfigTable", 3, ImGuiTableFlags.BordersV |
                                                             ImGuiTableFlags.BordersOuterV |
                                                             ImGuiTableFlags.BordersInnerV |
                                                             ImGuiTableFlags.BordersH |
                                                             ImGuiTableFlags.BordersOuterH |
                                                             ImGuiTableFlags.BordersInnerH))
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 0);
                    ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 1);
                    ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch, 100.0f, (uint) 2);
                    ImGui.TableHeadersRow();
                    if (filterConfigurations.Count == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted("No filters available.");
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
                            ImGui.TextUnformatted(filterConfiguration.Name);
                            ImGui.SameLine();
                        }

                        /*if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                        {
                            ImGui.PushFont(PluginFont.AppIcons.Value);
                            ImGui.Text(filterConfiguration.Icon);
                            ImGui.PopFont();
                        }*/

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(filterConfiguration.FormattedFilterType);
                        ImGui.TableNextColumn();
                        if (ImGui.SmallButton("Export Configuration##" + index))
                        {
                            var base64 = filterConfiguration.ExportBase64();
                            ImGui.SetClipboardText(base64);
                            PluginService.ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.PopStyleVar();
            }

            if (ImGui.CollapsingHeader("Import", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var importData = ImportData;
                if (ImGui.InputTextMultiline("Paste filter here",ref importData, 10000, new Vector2(400, 200) * ImGui.GetIO().FontGlobalScale))
                {
                    ImportData = importData;
                    ImportFailed = false;
                }

                if (ImGui.Button("Import##ImportBtn"))
                {
                    if (ImportData == "")
                    {
                        ImportFailed = true;
                        FailedReason =
                            "You must paste a filter generated via the export function before pressing import.";
                    }
                    else
                    {
                        if (FilterConfiguration.FromBase64(ImportData, out FilterConfiguration newFilter))
                        {
                            PluginService.PluginLogic.AddFilter(newFilter);
                        }
                        else
                        {
                            ImportFailed = true;
                            FailedReason =
                                "Invalid data detected in import string. Please make sure this string is valid.";
                        }
                    }
                }

                if (ImportFailed)
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, FailedReason);
                }
            }
            ImGui.PopID();                
        }

        public bool IsMenuItem => _isSeparator;
        public bool DrawBorder => true;

        public string FailedReason { get; set; } = "";

        public bool ImportFailed { get; set; } = false;

        public string ImportData { get; set; } = "";
    }
}