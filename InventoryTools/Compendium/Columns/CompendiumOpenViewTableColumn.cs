using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.Interface.FormFields;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Columns;

public class CompendiumOpenViewTableColumn<TData> : FormField<(string?, uint?), WindowState>, IValueColumn<WindowState, TData, MessageBase, (string?, uint?)>
{
    public delegate CompendiumOpenViewTableColumn<TData> Factory(CompendiumOpenViewTableColumnOptions<TData> columnOptions);

    private readonly ITextureProvider _textureProvider;
    private readonly CompendiumRowHeightSetting _rowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly MediatorService _mediatorService;
    private readonly Func<TData, (string?, uint?)> _valueSelector;
    private readonly Func<TData, uint> _rowIdSelector;
    private readonly ICompendiumType _compendiumType;
    private ISharedImmediateTexture? _texture;

    public CompendiumOpenViewTableColumn(
        ITextureProvider textureProvider,
        ImGuiService imGuiService,
        CompendiumRowHeightSetting rowHeightSetting,
        InventoryToolsConfiguration configuration,
        CompendiumOpenViewTableColumnOptions<TData> columnOptions,
        MediatorService mediatorService) : base(imGuiService)
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
        _rowIdSelector = columnOptions.RowIdSelector;
        _compendiumType = columnOptions.CompendiumType;

        _textureProvider = textureProvider;
        _rowHeightSetting = rowHeightSetting;
        _configuration = configuration;
        _mediatorService = mediatorService;
    }

    public virtual string? RenderName { get; set; }
    public virtual int Width { get; set; }
    public virtual bool HideFilter { get; set; }
    public bool IsHidden { get; set; }
    public virtual ImGuiTableColumnFlags ColumnFlags { get; set; }
    public IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var currentValue = this.CurrentValue(item);
            if (currentValue.Item2 != null)
            {
                bool isHq = currentValue.Item2 > 500000;
                currentValue.Item2 %= 500000;

                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().CellPadding.Y);
                if (ImGui.ImageButton(
                        this._textureProvider.GetFromGameIcon(new GameIconLookup((uint)currentValue.Item2, isHq))
                            .GetWrapOrEmpty().Handle,
                        this.IconSize * ImGui.GetIO().FontGlobalScale))
                {
                    this.OnButtonClick(item);
                }
            }
            else if (currentValue.Item1 != null)
            {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetStyle().CellPadding.Y);
                _texture ??= this.ImGuiService.LoadImage(currentValue.Item1);
                if (ImGui.ImageButton(
                        _texture.GetWrapOrEmpty().Handle,
                        this.IconSize * ImGui.GetIO().FontGlobalScale))
                {
                    this.OnButtonClick(item);
                }
            }
            else
            {
                ImGui.TextUnformatted(this.EmptyText);
            }
        }

        return null;
    }

    public bool DrawFilter(WindowState configuration, IColumn<WindowState, TData, MessageBase> column, int columnIndex)
    {
        return false;
    }

    public List<MessageBase> DrawFooter(WindowState config, List<TData> item, int columnIndex)
    {
        return [];
    }

    public void SetupFilter(IColumn<WindowState, TData, MessageBase> column, int columnIndex)
    {
    }

    public IEnumerable<TData> Sort(WindowState configuration, IEnumerable<TData> items, ImGuiSortDirection direction)
    {
        return items;
    }

    public IEnumerable<TData> Filter(WindowState config, IEnumerable<TData> items)
    {
        return items;
    }

    public string CsvExport(TData item)
    {
        var currentValue = CurrentValue(item);
        return currentValue.Item2?.ToString() ?? currentValue.Item1 ?? "";
    }

    public virtual string EmptyText { get; set; }

    public virtual Vector2 IconSize
    {
        get => new(_rowHeightSetting.CurrentValue(_configuration));
        set;
    }

    public virtual (string?, uint?) CurrentValue(TData item)
        => _valueSelector(item);

    public override (string?, uint?) CurrentValue(WindowState configuration)
    {
        return (null, null);
    }

    public virtual void OnButtonClick(TData item)
    {
        _mediatorService.Publish(new OpenCompendiumViewMessage(_compendiumType, _rowIdSelector.Invoke(item)));
    }

    public override bool DrawInput(WindowState configuration, int? inputSize = null)
    {
        return false;
    }

    public override void UpdateFilterConfiguration(WindowState configuration, (string?, uint?) newValue)
    {
    }


    public override (string?, uint?) DefaultValue { get; set; } = (null, null);
    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string HelpText { get; set; }
    public override string Version { get; set; }
    public override FormFieldType FieldType => FormFieldType.String;
}