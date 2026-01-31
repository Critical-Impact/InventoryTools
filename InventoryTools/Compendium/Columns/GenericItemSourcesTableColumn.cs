using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public class GenericItemSourcesTableColumn<TData> : StringColumn<WindowState, TData, MessageBase>
{
    public delegate GenericItemSourcesTableColumn<TData> Factory(CompendiumItemSourceColumnOptions<TData> columnOptions);

    private readonly ItemInfoRenderService _itemInfoRenderService;
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly Func<TData, List<ItemSource>> _valueSelector;

    public GenericItemSourcesTableColumn(ImGuiService imGuiService,
        StringColumnFilter stringColumnFilter,
        ItemInfoRenderService itemInfoRenderService,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        CompendiumItemSourceColumnOptions<TData> columnOptions) : base(imGuiService, stringColumnFilter)
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
        _itemInfoRenderService = itemInfoRenderService;
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
        var sources = string.Join(", ", _valueSelector.Invoke(item)
            .Select(c => _itemInfoRenderService.GetSourceTypeName(c.Type).Singular).Distinct());
        return sources;
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var messages = new  List<MessageBase>();
            var sources = _valueSelector.Invoke(item);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().CellPadding.Y);
            var rowHeight = _rowHeightSetting.CurrentValue(_configuration);
            messages.AddRange(_itemInfoRenderService.DrawItemSourceIconsContainer("ItemSources" + rowIndex, rowHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(rowHeight, rowHeight), sources.ToList()));
            return messages;
        }
        return null;
    }

    public override string HelpText { get; set; }
    public override string Version { get; }
}