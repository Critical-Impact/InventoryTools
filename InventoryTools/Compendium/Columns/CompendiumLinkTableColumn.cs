using System;
using System.Collections.Generic;
using System.Numerics;
using AllaganLib.Interface.Grid;
using AllaganLib.Interface.Grid.ColumnFilters;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Services;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Lumina.Excel;

namespace InventoryTools.Compendium.Columns;

/// <summary>
/// A string column that draws its value as an underlined link which opens the related compendium entry.
/// </summary>
public class CompendiumLinkTableColumn<TData> : StringColumn<WindowState, TData, MessageBase>
{
    public delegate CompendiumLinkTableColumn<TData> Factory(CompendiumLinkColumnOptions<TData> columnOptions);

    private readonly ICompendiumTypeFactory _compendiumTypeFactory;
    private readonly MediatorService _mediatorService;
    private readonly Func<TData, string?> _valueSelector;
    private readonly Func<TData, RowRef?> _rowRefSelector;
    private readonly Dictionary<(Type RowType, uint RowId), (ICompendiumType CompendiumType, uint RowId)?> _resolvedLinks = new();

    public CompendiumLinkTableColumn(ImGuiService imGuiService,
        StringColumnFilter stringColumnFilter,
        ICompendiumTypeFactory compendiumTypeFactory,
        MediatorService mediatorService,
        CompendiumLinkColumnOptions<TData> columnOptions) : base(imGuiService, stringColumnFilter)
    {
        _compendiumTypeFactory = compendiumTypeFactory;
        _mediatorService = mediatorService;
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
        _rowRefSelector = columnOptions.RowRefSelector;
    }

    public override string DefaultValue { get; set; } = string.Empty;
    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string? RenderName { get; set; }
    public override int Width { get; set; }
    public override bool HideFilter { get; set; }
    public override ImGuiTableColumnFlags ColumnFlags { get; set; }
    public override string EmptyText { get; set; }
    public override string HelpText { get; set; }
    public override string Version { get; set; }

    public override string? CurrentValue(TData item)
    {
        return _valueSelector(item);
    }

    public override IEnumerable<MessageBase>? Draw(WindowState config, TData item, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            return null;
        }

        var currentValue = CurrentValue(item);
        if (currentValue == null)
        {
            ImGui.TextUnformatted(EmptyText);
            return null;
        }

        ImGui.AlignTextToFramePadding();
        var link = ResolveLink(item);
        if (link == null)
        {
            ImGui.TextUnformatted(currentValue);
            return null;
        }

        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.TankBlue))
        {
            ImGui.TextUnformatted(currentValue);
        }

        var itemMin = ImGui.GetItemRectMin();
        var itemMax = ImGui.GetItemRectMax();
        ImGui.GetWindowDrawList().AddLine(new Vector2(itemMin.X, itemMax.Y), itemMax, ImGui.GetColorU32(ImGuiColors.TankBlue));

        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            using (ImRaii.Tooltip())
            {
                ImGui.TextUnformatted("Open " + link.Value.CompendiumType.Singular);
            }
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _mediatorService.Publish(new OpenCompendiumViewMessage(link.Value.CompendiumType, link.Value.RowId));
        }

        return null;
    }

    private (ICompendiumType CompendiumType, uint RowId)? ResolveLink(TData item)
    {
        var rowRef = _rowRefSelector(item);
        if (rowRef == null || rowRef.Value.RowType == null)
        {
            return null;
        }

        var key = (rowRef.Value.RowType, rowRef.Value.RowId);
        if (_resolvedLinks.TryGetValue(key, out var resolvedLink))
        {
            return resolvedLink;
        }

        resolvedLink = null;
        var compendiumType = _compendiumTypeFactory.GetByRowRef(rowRef.Value, out _);
        if (compendiumType != null)
        {
            var rowId = compendiumType.RemapType(rowRef.Value.RowType, rowRef.Value.RowId) ?? rowRef.Value.RowId;
            if (compendiumType.HasRow(rowId))
            {
                resolvedLink = (compendiumType, rowId);
            }
        }

        _resolvedLinks[key] = resolvedLink;
        return resolvedLink;
    }
}
