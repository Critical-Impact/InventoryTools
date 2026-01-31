using System;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Models;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public class GenericStringTableColumn<TData> : StringColumn<WindowState, TData, MessageBase>
{
    public delegate GenericStringTableColumn<TData> Factory(CompendiumStringColumnOptions<TData> columnOptions
    );

    private readonly Func<TData, string?> _valueSelector;

    public GenericStringTableColumn(ImGuiService imGuiService,
        StringColumnFilter stringColumnFilter,
        CompendiumStringColumnOptions<TData> columnOptions) : base(imGuiService, stringColumnFilter)
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
        return _valueSelector(item);
    }

    public override string HelpText { get; set; }
    public override string Version { get; }
}