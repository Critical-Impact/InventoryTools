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
using ImGuiNET;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestSelectedSecondaryItemColumn  : StringFormField<EquipmentSuggestConfig>,
    IValueColumn<EquipmentSuggestConfig, EquipmentSuggestItem, MessageBase, string?>
{
    public new ImGuiService ImGuiService { get; }
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ItemInfoRenderService _renderService;
    private readonly EquipmentSuggestViewModeSetting _viewModeSetting;
    private readonly EquipmentSuggestModeSetting _modeSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly ImGuiMenuService _menuService;
    private readonly IFont _font;

    public EquipmentSuggestSelectedSecondaryItemColumn(ImGuiService imGuiService, ImGuiTooltipService tooltipService,
        ItemInfoRenderService renderService, EquipmentSuggestViewModeSetting viewModeSetting,
        EquipmentSuggestModeSetting modeSetting, InventoryToolsConfiguration configuration,
        ImGuiMenuService menuService,
        IFont font) : base(imGuiService)
    {
        ImGuiService = imGuiService;
        _tooltipService = tooltipService;
        _renderService = renderService;
        _viewModeSetting = viewModeSetting;
        _modeSetting = modeSetting;
        _configuration = configuration;
        _menuService = menuService;
        _font = font;
    }

    public override string DefaultValue { get; set; } = string.Empty;
    public override string Key { get; set; } = "SecondarySelectedItem";
    public override string Name { get; set; } = "Off-hand";
    public string? RenderName { get; set; } = null;
    public int Width { get; set; } = 100;
    public bool HideFilter { get; set; } = true;
    public bool IsHidden {
        get => _modeSetting.CurrentValue(_configuration) != EquipmentSuggestMode.Tool;
        set { }
    }
    public ImGuiTableColumnFlags ColumnFlags { get; set; } = ImGuiTableColumnFlags.WidthFixed;
    public IEnumerable<MessageBase>? Draw(EquipmentSuggestConfig config, EquipmentSuggestItem item, int rowIndex, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var iconSize = _viewModeSetting.GetIconSize(_configuration);
            var containerSize = _viewModeSetting.GetIconContainerSize(_configuration);
            if (item.SecondarySelectedItem == null)
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
                                ImGuiService.GetIconTexture(item.SecondarySelectedItem.Item.Icon).ImGuiHandle,
                                new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                                new Vector2(0, 0), new Vector2(1, 1), 0))
                        {
                            item.SecondarySelectedItem = null;
                            return null;
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                        }
                        _tooltipService.DrawItemTooltip(item.SecondarySelectedItem);

                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow) && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                        {
                            ImGui.OpenPopup("RightClickMenu");
                        }

                        using (var popup = ImRaii.Popup("RightClickMenu"))
                        {
                            if (popup.Success)
                            {
                                messages.AddRange(ImGuiService.ImGuiMenuService.DrawRightClickPopup(item.SecondarySelectedItem));
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
                        ImGui.TextWrapped(item.SecondarySelectedItem.Item.NameString);
                        ImGui.PopTextWrapPos();
                    }
                }
                // using (var table = ImRaii.Table("ItemRow", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg))
                // {
                //     if (table)
                //     {
                //         ImGui.TableNextRow();
                //
                //         ImGui.TableSetColumnIndex(0);
                //         {
                //             using (var group = ImRaii.Group())
                //             {
                //                 if (group)
                //                 {
                //                     if (ImGui.ImageButton(
                //                             ImGuiService.GetIconTexture(item.SecondarySelectedItem.Item.Icon).ImGuiHandle,
                //                             new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                //                             new Vector2(0, 0), new Vector2(1, 1), 0))
                //                     {
                //                         item.SecondarySelectedItem = null;
                //                         return null;
                //                     }
                //                     _tooltipService.DrawItemTooltip(item.SecondarySelectedItem);
                //                 }
                //             }
                //             ImGui.SameLine();
                //             using (var group = ImRaii.Group())
                //             {
                //                 if (group)
                //                 {
                //
                //                     ImGui.PushTextWrapPos();
                //                     ImGui.TextWrapped(item.SecondarySelectedItem.Item.NameString);
                //                     ImGui.PopTextWrapPos();
                //                 }
                //             }
                //         }
                //
                //         ImGui.TableSetColumnIndex(1);
                //         {
                //             var sources = item.SecondarySelectedItem.Item.Sources;
                //             var groupedSources = _renderService.GetGroupedSources(sources);
                //             ImGuiService.WrapTableColumnElements("Items", groupedSources,
                //                 iconSize * ImGui.GetIO().FontGlobalScale, containerSize * ImGui.GetIO().FontGlobalScale,
                //                 groupedSource =>
                //                 {
                //                     var firstItem = groupedSource[0];
                //                     var icon = _renderService.GetSourceIcon(firstItem);
                //                     var sourceIcon = ImGuiService.GetIconTexture(icon);
                //                     var tint = firstItem.Type == item.SecondaryAcquisitionSource
                //                         ? Vector4.One
                //                         : new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
                //                     if (ImGui.ImageButton(sourceIcon.ImGuiHandle,
                //                             new Vector2(iconSize, iconSize) * ImGui.GetIO().FontGlobalScale,
                //                             new Vector2(0, 0),
                //                             new Vector2(1, 1), 0, Vector4.Zero, tint))
                //                     {
                //                         if (item.SecondaryAcquisitionSource == firstItem.Type)
                //                         {
                //                             item.SecondaryAcquisitionSource = null;
                //                         }
                //                         else
                //                         {
                //                             item.SecondaryAcquisitionSource = firstItem.Type;
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

    public List<MessageBase> DrawFooter(EquipmentSuggestConfig config, List<EquipmentSuggestItem> item, int columnIndex)
    {
        ImGui.TableNextColumn();
        return new();
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