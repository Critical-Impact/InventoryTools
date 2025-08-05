using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.Interface.FormFields;
using AllaganLib.Interface.Grid;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestSelectedItemColumn  : StringFormField<EquipmentSuggestConfig>,
    IValueColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase, string?>
{
    public new ImGuiService ImGuiService { get; }
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ItemInfoRenderService _renderService;
    private readonly EquipmentSuggestViewModeSetting _viewModeSetting;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IListService _listService;
    private readonly Lazy<EquipmentSuggestGrid> _equipmentSuggestGrid;
    private readonly IFont _font;

    public EquipmentSuggestSelectedItemColumn(ImGuiService imGuiService, ImGuiTooltipService tooltipService,
        ItemInfoRenderService renderService, EquipmentSuggestViewModeSetting viewModeSetting,
        EquipmentSuggestModeSetting modeSetting, InventoryToolsConfiguration configuration, IListService listService,
        Lazy<EquipmentSuggestGrid> equipmentSuggestGrid,
        IFont font) : base(imGuiService)
    {
        ImGuiService = imGuiService;
        _tooltipService = tooltipService;
        _renderService = renderService;
        _viewModeSetting = viewModeSetting;
        _modeSetting = modeSetting;
        _configuration = configuration;
        _listService = listService;
        _equipmentSuggestGrid = equipmentSuggestGrid;
        _font = font;
    }

    public override string DefaultValue { get; set; } = string.Empty;
    public override string Key { get; set; } = "SelectedItem";
    public override string Name {
        get => _modeSetting.CurrentValue(_configuration) == EquipmentSuggestMode.Tool ? "Main Hand" : "Selected Item";
        set { }
    }
    public string? RenderName { get; set; } = null;
    public int Width { get; set; } = 100;
    public bool HideFilter { get; set; } = true;
    public bool IsHidden { get; set; } = false;
    public ImGuiTableColumnFlags ColumnFlags { get; set; } = ImGuiTableColumnFlags.WidthFixed;
    public IEnumerable<MessageBase>? Draw(EquipmentSuggestConfig config, EquipmentSuggestItem item, int rowIndex, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var iconSize = _viewModeSetting.GetIconSize(_configuration);
            var containerSize = _viewModeSetting.GetIconContainerSize(_configuration);
            if (item.SelectedItem == null)
            {
                ImGui.Text("No item selected");
            }
            else
            {

                using (var group = ImRaii.Group())
                {
                    if (group)
                    {
                        if (ImGui.ImageButton(
                                ImGuiService.GetIconTexture(item.SelectedItem.Item.Icon).Handle,
                                new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                                new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            item.SelectedItem = null;
                            return null;
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        }
                        _tooltipService.DrawItemTooltip(item.SelectedItem);
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickMenu");
                        }

                        using (var popup = ImRaii.Popup("RightClickMenu"))
                        {
                            if (popup.Success)
                            {
                                messages.AddRange(ImGuiService.ImGuiMenuService.DrawRightClickPopup(item.SelectedItem));
                            }
                        }
                    }
                }
                ImGui.SameLine();
                using (var group = ImRaii.Group())
                {
                    if (group)
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.TextWrapped(item.SelectedItem.Item.NameString);
                        ImGui.PopTextWrapPos();
                    }
                }

                // using(var table = ImRaii.Table("ItemRow", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
                // {
                //     if (table)
                //     {
                //
                //         ImGui.TableNextRow();
                //
                //         ImGui.TableSetColumnIndex(0);
                //         {
                //             using (var group = ImRaii.Group())
                //             {
                //                 if (group)
                //                 {
                //                     if (ImGui.ImageButton(
                //                             ImGuiService.GetIconTexture(item.SelectedItem.Item.Icon).Handle,
                //                             new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                //                             new Vector2(0, 0), new Vector2(1, 1), 0))
                //                     {
                //                         item.SelectedItem = null;
                //                         return null;
                //                     }
                //                     _tooltipService.DrawItemTooltip(item.SelectedItem);
                //                 }
                //             }
                //             ImGui.SameLine();
                //             using (var group = ImRaii.Group())
                //             {
                //                 if (group)
                //                 {
                //                     ImGui.PushTextWrapPos();
                //                     ImGui.TextWrapped(item.SelectedItem.Item.NameString);
                //                     ImGui.PopTextWrapPos();
                //                 }
                //             }
                //         }
                //
                //         ImGui.TableSetColumnIndex(1);
                //         {
                //             var sources = item.SelectedItem.Item.Sources;
                //             var groupedSources = _renderService.GetGroupedSources(sources);
                //             ImGuiService.WrapTableColumnElements("Items", groupedSources,
                //                 iconSize * ImGui.GetIO().FontGlobalScale, containerSize * ImGui.GetIO().FontGlobalScale,
                //                 groupedSource =>
                //                 {
                //                     var firstItem = groupedSource[0];
                //                     var icon = _renderService.GetSourceIcon(firstItem);
                //                     var sourceIcon = ImGuiService.GetIconTexture(icon);
                //                     var tint = firstItem.Type == item.AcquisitionSource
                //                         ? Vector4.One
                //                         : new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
                //                     if (ImGui.ImageButton(sourceIcon.Handle,
                //                             new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                //                             new Vector2(0, 0),
                //                             new Vector2(1, 1), 0, Vector4.Zero, tint))
                //                     {
                //                         if (item.AcquisitionSource == firstItem.Type)
                //                         {
                //                             item.AcquisitionSource = null;
                //                         }
                //                         else
                //                         {
                //                             item.AcquisitionSource = firstItem.Type;
                //                         }
                //                     }
                //
                //                     _renderService.DrawItemSourceTooltip(RendererType.Source, groupedSource);
                //                     return true;
                //                 });
                //
                //         }
                //     }
                // }
            }
        }

        return messages;
    }

    public bool DrawFilter(EquipmentSuggestConfig configuration, IColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase> column, int columnIndex)
    {
        return false;
    }

    private List<SearchResult> GetItems()
    {
        return _equipmentSuggestGrid.Value.GetItems().SelectMany<EquipmentSuggestItem, SearchResult?>(c => [c.SelectedItem, c.SecondarySelectedItem]).Where(c => c != null).Select(c => c!).ToList();
    }

    public List<MessageBase> DrawFooter(EquipmentSuggestConfig config, List<EquipmentSuggestItem> item, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var currentCursorX = ImGui.GetCursorPosX();
            if (ImGuiService.DrawIconButton(_font, FontAwesomeIcon.Plus, ref currentCursorX))
            {
                ImGui.OpenPopup("AddToCraftList");
            }
        }

        using (var popup = ImRaii.Popup("AddToCraftList"))
        {
            if (popup)
            {
                var craftFilters =
                    _listService.Lists.Where(c =>
                        c.FilterType == Logic.FilterType.CraftFilter && !c.CraftListDefault).ToArray();
                if (craftFilters.Length != 0)
                {
                    using var menu = ImRaii.Menu("Add to Craft List");
                    if(menu)
                    {
                        foreach (var filter in craftFilters)
                        {
                            if (!ImGui.Selectable(filter.Name)) continue;
                            foreach (var toAdd in GetItems())
                            {
                                filter.CraftList.AddCraftItem(toAdd.Item.RowId);
                            }
                            messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                            messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                            filter.NeedsRefresh = true;
                        }
                    }
                }

                if (ImGui.Selectable("Add to new Craft List"))
                {
                    var filter = _listService.AddNewCraftList();
                    foreach (var toAdd in GetItems())
                    {
                        filter.CraftList.AddCraftItem(toAdd.Item.RowId);
                    }
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    filter.NeedsRefresh = true;
                }
                if (ImGui.Selectable("Add to new Craft List (ephemeral)"))
                {
                    var filter = _listService.AddNewCraftList(null,true);
                    foreach (var toAdd in GetItems())
                    {
                        filter.CraftList.AddCraftItem(toAdd.Item.RowId);
                    }
                    messages.Add(new OpenGenericWindowMessage(typeof(CraftsWindow)));
                    messages.Add(new FocusListMessage(typeof(CraftsWindow), filter));
                    filter.NeedsRefresh = true;
                }
                ImGui.Separator();
                var curatedLists =
                    _listService.Lists.Where(c => c.FilterType == FilterType.CuratedList).ToArray();
                if (curatedLists.Length != 0)
                {
                    using var menu = ImRaii.Menu("Add to Curated List");
                    if(menu)
                    {
                        foreach (var filter in curatedLists)
                        {
                            if (!ImGui.MenuItem(filter.Name)) continue;
                            foreach (var toAdd in GetItems())
                            {
                                filter.AddCuratedItem(new CuratedItem(toAdd.Item.RowId));
                            }
                            messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
                            filter.NeedsRefresh = true;
                        }
                    }
                }

                if (ImGui.Selectable("Add to new Curated List"))
                {
                    var filter = _listService.AddNewCuratedList();
                    foreach (var toAdd in GetItems())
                    {
                        filter.AddCuratedItem(new CuratedItem(toAdd.Item.RowId));
                    }
                    messages.Add(new FocusListMessage(typeof(FiltersWindow), filter));
                    filter.NeedsRefresh = true;
                }

            }
        }

        return messages;
    }

    public void SetupFilter(IColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase> column, int columnIndex)
    {
    }

    public IEnumerable<EquipmentSuggestItem> Sort(EquipmentSuggestConfig configuration, IEnumerable<EquipmentSuggestItem> items, ImGuiSortDirection direction)
    {
        return items;
    }

    public IEnumerable<EquipmentSuggestItem> Filter(EquipmentSuggestConfig config, IEnumerable<EquipmentSuggestItem> items)
    {
        return items;
    }

    public string CsvExport(EquipmentSuggestItem item)
    {
        return "";
    }

    public override string HelpText { get; set; } = "The item you've selected from the list of recommendations";
    public override string Version { get; } = "1.12.0.10";

    public string? CurrentValue(EquipmentSuggestItem item)
    {
        return null;
    }
}