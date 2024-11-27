using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class ImportExportPage : Page
    {
        private readonly IListService _listService;
        private readonly IChatUtilities _chatUtilities;
        private readonly PluginLogic _pluginLogic;
        private readonly ListImportExportService _importExportService;
        private readonly IClipboardService _clipboardService;

        public ImportExportPage(ILogger<ImportExportPage> logger, ImGuiService imGuiService, IListService listService, IChatUtilities chatUtilities, PluginLogic pluginLogic, ListImportExportService importExportService, IClipboardService clipboardService) : base(logger, imGuiService)
        {
            _listService = listService;
            _chatUtilities = chatUtilities;
            _pluginLogic = pluginLogic;
            _importExportService = importExportService;
            _clipboardService = clipboardService;
        }
        private bool _isSeparator = false;
        public override void Initialize()
        {

        }

        public override string Name { get; } =  "Import/Export";
        public override List<MessageBase>? Draw()
        {
            ImGui.PushID("ImportSection");
            if (ImGui.CollapsingHeader("Export", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var filterConfigurations = _listService.Lists;
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
                        ImGui.TextUnformatted("No lists created yet!");
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
                            var base64 = _importExportService.ToBase64(filterConfiguration);
                            _clipboardService.CopyToClipboard(base64);
                            _chatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                        }
                    }

                    ImGui.EndTable();
                }

                ImGui.PopStyleVar();
            }

            if (ImGui.CollapsingHeader("Import", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                var importData = ImportData;
                if (ImGui.InputTextMultiline("Paste list here",ref importData, 10000, new Vector2(400, 200) * ImGui.GetIO().FontGlobalScale))
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
                            "You must paste a list generated via the export function before pressing import.";
                    }
                    else
                    {
                        if (_importExportService.FromBase64(ImportData, out FilterConfiguration newFilter))
                        {
                            _pluginLogic.AddFilter(newFilter);
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
            return null;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;

        public string FailedReason { get; set; } = "";

        public bool ImportFailed { get; set; } = false;

        public string ImportData { get; set; } = "";
    }
}