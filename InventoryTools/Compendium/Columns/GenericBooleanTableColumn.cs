using System;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Models;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public class GenericBooleanTableColumn<TData> : BooleanColumn<WindowState, TData, MessageBase>
{
    public delegate GenericBooleanTableColumn<TData> Factory(CompendiumBooleanColumnOptions<TData> columnOptions);

    private readonly Func<TData, bool?> _valueSelector;

    public GenericBooleanTableColumn(ImGuiService imGuiService,
        CompendiumBooleanColumnOptions<TData> columnOptions) : base(imGuiService)
    {
        Key = columnOptions.Key;
        Name = columnOptions.Name;
        RenderName = columnOptions.RenderName;
        Width = columnOptions.Width;
        HideFilter = columnOptions.HideFilter;
        ColumnFlags = columnOptions.ColumnFlags;
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

    public override string? CurrentValue(TData item)
    {
        var value = _valueSelector(item);
        if (value == null)
        {
            return null;
        }
        return value == true ? "true" : "false";
    }

    public override string HelpText { get; set; }
    public override string Version { get; }
}