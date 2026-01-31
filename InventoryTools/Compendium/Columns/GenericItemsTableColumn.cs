using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using OtterGui;

namespace InventoryTools.Compendium.Columns;

public class GenericItemsTableColumn<TData> : StringColumn<WindowState, TData, MessageBase>
{
    public delegate GenericItemsTableColumn<TData> Factory(CompendiumItemsColumnOptions<TData> columnOptions);

    private readonly ImGuiService _imGuiService;
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly Func<TData, List<ItemRow>> _valueSelector;
    private readonly CompendiumDataCacher<TData, List<ItemRow>> _cache = new(50);

    public GenericItemsTableColumn(ImGuiService imGuiService,
        StringColumnFilter stringColumnFilter,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        CompendiumItemsColumnOptions<TData> columnOptions) : base(imGuiService, stringColumnFilter)
    {
        Key = columnOptions.Key;
        Name = columnOptions.Name;
        RenderName = columnOptions.RenderName;
        Width = columnOptions.Width;
        HideFilter = columnOptions.HideFilter;
        ColumnFlags = columnOptions.ColumnFlags;
        EmptyText = columnOptions.EmptyText;
        HelpText = columnOptions.HelpText;
        Version = columnOptions.Version;
        _valueSelector = columnOptions.ValueSelector;
        _imGuiService = imGuiService;
        _rowHeightSetting = rowHeightSetting;
        _configuration = configuration;
    }

    public override string DefaultValue { get; set; }
    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string? RenderName { get; set; }
    public override int Width { get; set; }
    public override bool HideFilter { get; set; }
    public override ImGuiTableColumnFlags ColumnFlags { get; set; }
    public override string EmptyText { get; set; }
    public override string? CurrentValue(TData item)
    {
        var sources = string.Join(", ", _valueSelector.Invoke(item).Select(c => c.NameString).Distinct());
        return sources;
    }

    private List<ItemRow> GetRows(TData item)
    {
        if (_cache.TryGet(item, out var rows))
            return rows;

        rows = _valueSelector.Invoke(item);
        _cache.Add(item, rows);
        return rows;
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var messages = new  List<MessageBase>();
            var items = GetRows(item);
            var rowHeight = _rowHeightSetting.CurrentValue(_configuration);
            _imGuiService.WrapTableColumnElements("Items" + Name, items,
            rowHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X,
            item =>
            {
                ImGui.Image(_imGuiService.GetIconTexture(item.Icon).Handle,
                    new Vector2(rowHeight) * ImGui.GetIO().FontGlobalScale);
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("RightClick" + item.RowId);
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled &
                                        ImGuiHoveredFlags.AllowWhenOverlapped &
                                        ImGuiHoveredFlags.AllowWhenBlockedByPopup &
                                        ImGuiHoveredFlags.AllowWhenBlockedByActiveItem &
                                        ImGuiHoveredFlags.AnyWindow) &&
                    ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), item.RowId));
                }

                using(var popup = ImRaii.Popup("RightClick" + item.RowId))
                {
                    if (popup)
                    {
                        messages.AddRange(_imGuiService.ImGuiMenuService.DrawRightClickPopup(item));
                    }
                }
                ImGuiUtil.HoverTooltip(item.NameString);

                return true;
            });
            return messages;
        }
        return null;
    }

    public override string HelpText { get; set; }
    public override string Version { get; }
}