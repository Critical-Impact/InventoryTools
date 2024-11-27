using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class CraftFiltersPage : Page
    {
        private readonly IListService _listService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ListImportExportService _importExportService;
        private readonly IClipboardService _clipboardService;

        public CraftFiltersPage(ILogger<CraftFiltersPage> logger, ImGuiService imGuiService, IListService listService, IChatUtilities chatUtilities, ListImportExportService importExportService, IClipboardService clipboardService) : base(logger, imGuiService)
        {
            _listService = listService;
            _chatUtilities = chatUtilities;
            _importExportService = importExportService;
            _clipboardService = clipboardService;
        }
        private bool _isSeparator = false;
        public override void Initialize()
        {

        }

        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();
        public Widgets.PopupMenu GetFilterMenu(FilterConfiguration configuration)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new Widgets.PopupMenu("fm" + configuration.Key, Widgets.PopupMenu.PopupMenuButtons.LeftRight,
                    new List<Widgets.PopupMenu.IPopupMenuItem>()
                    {
                        new Widgets.PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateList, "Duplicate the list."),
                        new Widgets.PopupMenu.PopupMenuItemSelectable("Export Configuration", "ef_" + configuration.Key,ExportList, "Exports the list."),
                        new Widgets.PopupMenu.PopupMenuItemSelectable( "Move Up", "mu_" + configuration.Key, MoveListUp,  "Move the list up."),
                        new Widgets.PopupMenu.PopupMenuItemSelectable( "Move Down", "md_" + configuration.Key, MoveListDown, "Move the list down."),
                        new Widgets.PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this list?", RemoveList, "Remove the list."),
                    }
                );
            }
            return _popupMenus[configuration];
        }

        private void RemoveList(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = _listService.GetListByKey(id);
                if (existingFilter != null)
                {
                    _listService.RemoveList(existingFilter);

                }
            }
        }

        private void MoveListUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListUp(existingFilter);
            }
        }

        private void MoveListDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListDown(existingFilter);
            }
        }

        private void ExportList(string id)
        {
            id = id.Replace("ef_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                var base64 = _importExportService.ToBase64(existingFilter);
                _clipboardService.CopyToClipboard(base64);
                _chatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
            }
        }

        private void DuplicateList(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.DuplicateList(existingFilter, filterName);
            }
        }

        public override string Name { get; } = "Craft Lists";
        public override List<MessageBase>? Draw()
        {
            var messages = new List<MessageBase>();
            var filterConfigurations = _listService.Lists.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault).ToList();
            if (ImGui.CollapsingHeader("Craft Lists", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
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
                        ImGui.TextUnformatted("No craft lists created yet!");
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

                        ImGui.Button("...");
                        GetFilterMenu(filterConfiguration).Draw();
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