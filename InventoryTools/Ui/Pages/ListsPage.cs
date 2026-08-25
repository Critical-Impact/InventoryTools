using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services;
using DalaMock.Host.Mediator;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class ListsPage : Page
    {
        private readonly IListService _listService;
        private readonly IChatUtilities _chatUtilities;
        private readonly ListImportExportService _importExportService;
        private readonly IClipboardService _clipboardService;

        public ListsPage(ILogger<ListsPage> logger, ImGuiService imGuiService, IListService listService, IChatUtilities chatUtilities, ListImportExportService importExportService, IClipboardService clipboardService) : base(logger, imGuiService)
        {
            _listService = listService;
            _chatUtilities = chatUtilities;
            _importExportService = importExportService;
            _clipboardService = clipboardService;
        }

        private string? _draggedItemKey;
        private string? _draggedSection;
        private string _itemListImportError = string.Empty;
        private string _craftListImportError = string.Empty;
        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();

        public override void Initialize() { }

        public override string Key { get; } = "data/lists";
        public override string Name { get; } = "Lists";

        public PopupMenu GetListMenu(FilterConfiguration configuration)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new PopupMenu("fm" + configuration.Key, PopupMenu.PopupMenuButtons.LeftRight,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateList, "Duplicate the list."),
                        new PopupMenu.PopupMenuItemSelectable("Export Configuration", "ef_" + configuration.Key, ExportList, "Copies the list export string to clipboard."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this list?", RemoveList, "Remove the list."),
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

        private void ImportFromClipboard(bool isCraftList)
        {
            var text = _clipboardService.PasteFromClipboard();
            if (!string.IsNullOrWhiteSpace(text) && _importExportService.FromBase64(text, out var config))
            {
                _listService.AddList(config);
                if (isCraftList)
                {
                    _craftListImportError = string.Empty;
                }
                else
                {
                    _itemListImportError = string.Empty;
                }
            }
            else
            {
                var error = "Invalid or incompatible list data in clipboard.";
                if (isCraftList)
                {
                    _craftListImportError = error;
                }
                else
                {
                    _itemListImportError = error;
                }
            }
        }

        private void DrawListRow(List<FilterConfiguration> lists, int index, string payloadId, bool showType)
        {
            var config = lists[index];
            var key = config.Key;

            using (ImRaii.PushId("ListRow" + key))
            {
                ImGui.Button("=");

                using (var source = ImRaii.DragDropSource())
                {
                    if (source)
                    {
                        _draggedItemKey = key;
                        _draggedSection = payloadId;
                        ImGui.SetDragDropPayload(payloadId, []);
                        ImGui.TextUnformatted("Moving: " + (config.Name != "" ? config.Name : "Untitled"));
                    }
                }

                using (var target = ImRaii.DragDropTarget())
                {
                    if (target)
                    {
                        if (OtterGui.ImGuiUtil.IsDropping(payloadId) && _draggedItemKey != null && _draggedSection == payloadId)
                        {
                            var sourceIndex = lists.FindIndex(c => c.Key == _draggedItemKey);
                            if (sourceIndex >= 0 && sourceIndex != index)
                            {
                                var delta = index - sourceIndex;
                                var dragged = lists[sourceIndex];
                                if (delta > 0)
                                {
                                    for (var i = 0; i < delta; i++)
                                    {
                                        _listService.MoveListDown(dragged);
                                    }
                                }
                                else
                                {
                                    for (var i = 0; i < -delta; i++)
                                    {
                                        _listService.MoveListUp(dragged);
                                    }
                                }
                            }
                            _draggedItemKey = null;
                            _draggedSection = null;
                        }
                    }
                }

                ImGui.SameLine();

                var displayName = config.Name != "" ? config.Name : "Untitled";
                ImGui.TextUnformatted(displayName);

                if (showType)
                {
                    ImGui.SameLine();
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
                    {
                        ImGui.TextUnformatted("[" + config.FormattedFilterType + "]");
                    }
                }

                var menuLabel = "...##" + key;
                var menuWidth = ImGui.CalcTextSize("...").X + ImGui.GetStyle().FramePadding.X * 2;
                ImGui.SameLine(ImGui.GetContentRegionMax().X - menuWidth);
                ImGui.Button(menuLabel);
                GetListMenu(config).Draw();
            }

            ImGui.Separator();
        }

        public override List<MessageBase>? Draw()
        {
            var messages = new List<MessageBase>();

            var itemLists = _listService.Lists
                .Where(c => c.FilterType != FilterType.CraftFilter)
                .ToList();

            var craftLists = _listService.Lists
                .Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault)
                .ToList();

            if (ImGui.CollapsingHeader("Item Lists", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (ImGui.Button("Import from Clipboard##itemlist"))
                {
                    ImportFromClipboard(false);
                }

                if (!string.IsNullOrEmpty(_itemListImportError))
                {
                    ImGui.SameLine();
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                    {
                        ImGui.TextUnformatted(_itemListImportError);
                    }
                }

                ImGui.Spacing();

                if (itemLists.Count == 0)
                {
                    ImGui.TextUnformatted("No item lists created yet!");
                }
                else
                {
                    for (var i = 0; i < itemLists.Count; i++)
                    {
                        DrawListRow(itemLists, i, "##ItemListReorder", true);
                    }
                }
            }

            ImGui.Spacing();

            if (ImGui.CollapsingHeader("Craft Lists", ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
            {
                if (ImGui.Button("Import from Clipboard##craftlist"))
                {
                    ImportFromClipboard(true);
                }

                if (!string.IsNullOrEmpty(_craftListImportError))
                {
                    ImGui.SameLine();
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                    {
                        ImGui.TextUnformatted(_craftListImportError);
                    }
                }

                ImGui.Spacing();

                if (craftLists.Count == 0)
                {
                    ImGui.TextUnformatted("No craft lists created yet!");
                }
                else
                {
                    for (var i = 0; i < craftLists.Count; i++)
                    {
                        DrawListRow(craftLists, i, "##CraftListReorder", false);
                    }
                }
            }

            return messages;
        }

        public override bool IsMenuItem => false;
        public override bool DrawBorder => true;
    }
}
