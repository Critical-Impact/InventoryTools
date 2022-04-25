using System.Numerics;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public static class ImportExportSection
    {
        public static string ImportData = "";
        public static bool ImportFailed = false;
        public static string FailedReason = "";
        
        public static void Draw()
        {
            ImGui.PushID("ImportSection");
            var pluginLogic = PluginService.PluginLogic;
            if (ImGui.CollapsingHeader("Export"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5, 5));
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
                    if (pluginLogic.FilterConfigurations.Count == 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.Text("No filters available.");
                        ImGui.TableNextColumn();
                        ImGui.TableNextColumn();
                    }

                    for (var index = 0; index < pluginLogic.FilterConfigurations.Count; index++)
                    {
                        ImGui.TableNextRow();
                        var filterConfiguration = pluginLogic.FilterConfigurations[index];
                        ImGui.TableNextColumn();
                        if (filterConfiguration.Name != "")
                        {
                            ImGui.Text(filterConfiguration.Name);
                            ImGui.SameLine();
                        }

                        /*if (PluginFont.AppIcons.HasValue && filterConfiguration.Icon != null)
                        {
                            ImGui.PushFont(PluginFont.AppIcons.Value);
                            ImGui.Text(filterConfiguration.Icon);
                            ImGui.PopFont();
                        }*/

                        ImGui.TableNextColumn();
                        ImGui.Text(filterConfiguration.FormattedFilterType);
                        ImGui.TableNextColumn();
                        if (ImGui.SmallButton("Export Configuration##" + index))
                        {
                            var base64 = filterConfiguration.ExportBase64();
                            ImGui.SetClipboardText(base64);
                            ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.PopStyleVar();
            }

            if (ImGui.CollapsingHeader("Import"))
            {
                var importData = ImportData;
                if (ImGui.InputTextMultiline("Paste filter here",ref importData, 5000, new Vector2(400, 200)))
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
    }
}