using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;

namespace InventoryTools.Compendium.Columns;

public class GenericItemTableColumn<TData> : IconColumn<WindowState, TData, MessageBase>
{
    public delegate GenericItemTableColumn<TData> Factory(CompendiumItemColumnOptions<TData> columnOptions);

    private readonly ITextureProvider _textureProvider;
    private readonly ItemSheet _itemSheet;
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ImGuiMenuService _imGuiMenuService;
    private readonly Func<TData, uint?> _valueSelector;
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;

    public GenericItemTableColumn(ITextureProvider textureProvider,
        ImGuiService imGuiService,
        ItemSheet itemSheet,
        ImGuiTooltipService tooltipService,
        ImGuiMenuService imGuiMenuService,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        CompendiumItemColumnOptions<TData> columnOptions) : base(textureProvider, imGuiService)
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
        _textureProvider = textureProvider;
        _itemSheet = itemSheet;
        _tooltipService = tooltipService;
        _imGuiMenuService = imGuiMenuService;
        _rowHeightSetting = rowHeightSetting;
        _configuration = configuration;
    }

    public override int DefaultValue { get; set; }
    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string? RenderName { get; set; }
    public override int Width { get; set; }
    public override bool HideFilter { get; set; }
    public override ImGuiTableColumnFlags ColumnFlags { get; set; }
    public override string EmptyText { get; set; }
    public override Vector2 IconSize
    {
        get => new(_rowHeightSetting.CurrentValue(_configuration));
        set;
    }

    public override int? CurrentValue(TData item)
    {
        return (int?)_valueSelector.Invoke(item);
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        var messages = new List<MessageBase>();
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var currentValue = this.CurrentValue(item);
            if (currentValue != null)
            {
                var itemRow = _itemSheet.GetRow((uint)currentValue.Value);
                bool isHq = currentValue > 500000;
                currentValue %= 500000;

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().CellPadding.Y);
                ImGui.Image(this._textureProvider.GetFromGameIcon(new GameIconLookup(itemRow.Icon, isHq)).GetWrapOrEmpty().Handle, this.IconSize * ImGui.GetIO().FontGlobalScale);
                if (ImGui.IsItemHovered())
                {

                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), itemRow.RowId));
                    }
                    else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        ImGui.OpenPopup("GSIP_" + currentValue);
                    }
                    else
                    {
                        _tooltipService.DrawItemTooltip(new SearchResult(itemRow));
                    }
                }

                using (var popup = ImRaii.Popup("GSIP_" +currentValue))
                {
                    if (popup)
                    {
                        _imGuiMenuService.DrawMenuItems(new SearchResult(itemRow), messages);
                    }
                }
            }
            else
            {
                ImGui.TextUnformatted(this.EmptyText);
            }
        }

        return messages;
    }

    public override string HelpText { get; set; }
    public override string Version { get; }
}