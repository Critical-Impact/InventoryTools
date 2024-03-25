using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class FiltersPage : Page
    {
        private readonly IListService _listService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ListImportExportService _importExportService;

        public FiltersPage(ILogger<FiltersPage> logger, ImGuiService imGuiService, IListService listService, IChatUtilities chatUtilities, ListImportExportService importExportService) : base(logger, imGuiService)
        {
            _listService = listService;
            _chatUtilities = chatUtilities;
            _importExportService = importExportService;
        }
        private bool _isSeparator = false;
        public override void Initialize()
        {
        }

        public override string Name { get; } = "Item Lists";
        public override List<MessageBase>? Draw()
        {
            var messages = new List<MessageBase>();
            var filterConfigurations = _listService.Lists.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
            if (ImGui.CollapsingHeader("Item Lists", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
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
                        ImGui.TextUnformatted("No item lists created yet!");
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
                            ImGui.TextUnformatted(filterConfiguration.Name);
                            ImGui.SameLine();
                        }

                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(filterConfiguration.FormattedFilterType);

                        ImGui.TableNextColumn();
                        if (ImGui.SmallButton("Up##" + index))
                        {
                            _listService.MoveListUp(filterConfiguration);
                        }
                        ImGui.SameLine();
                        if (ImGui.SmallButton("Down##" + index))
                        {
                            _listService.MoveListDown(filterConfiguration);
                        }
                        
                        ImGui.TableNextColumn();
                        if (ImGui.SmallButton("Export Configuration##" + index))
                        {
                            var base64 = _importExportService.ToBase64(filterConfiguration);
                            ImGui.SetClipboardText(base64);
                            _chatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
                        }

                        if (ImGui.SmallButton("Remove##" + index))
                        {
                            ImGui.OpenPopup("Delete?##" + index);
                        }

                        if (ImGui.SmallButton("Open as Window##" + index))
                        {
                            messages.Add(new OpenStringWindowMessage(typeof(FilterWindow), filterConfiguration.Key));
                        }

                        if (ImGui.BeginPopupModal("Delete?##" + index))
                        {
                            ImGui.TextUnformatted(
                                "Are you sure you want to delete this filter?.\nThis operation cannot be undone!\n\n");
                            ImGui.Separator();

                            if (ImGui.Button("OK", new Vector2(120, 0) * ImGui.GetIO().FontGlobalScale))
                            {
                                _listService.RemoveList(filterConfiguration);
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

            return messages;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;
    }
}