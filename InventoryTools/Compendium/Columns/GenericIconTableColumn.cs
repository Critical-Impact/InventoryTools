using System;
using System.Numerics;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public class GenericIconTableColumn<TData> : IconColumn<WindowState, TData, MessageBase>
{
    public delegate GenericIconTableColumn<TData> Factory(CompendiumIconColumnOptions<TData> columnOptions);

    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly Func<TData, int?> _valueSelector;

    public GenericIconTableColumn(
        ITextureProvider textureProvider,
        ImGuiService imGuiService,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        CompendiumIconColumnOptions<TData> columnOptions)
        : base(textureProvider, imGuiService)
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
        => _valueSelector(item);

    public override string HelpText { get; set; }
    public override string Version { get; }
}