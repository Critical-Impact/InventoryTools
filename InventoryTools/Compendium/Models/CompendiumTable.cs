using System;
using System.Collections.Generic;
using AllaganLib.Data.Service;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Logic.Settings;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumTable<TData> : RenderTable<WindowState, TData, MessageBase>, ICompendiumTable
{
    private readonly ICompendiumType<TData> _compendiumType;
    private readonly CompendiumRowHeightSetting _compendiumRowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;

    public delegate CompendiumTable<TData> Factory(CompendiumTableOptions<TData> tableOptions);

    public CompendiumTable(CompendiumTableOptions<TData> tableOptions, CompendiumRowHeightSetting compendiumRowHeightSetting, InventoryToolsConfiguration configuration, CsvLoaderService csvLoaderService, WindowState searchFilter) : base(csvLoaderService, searchFilter, tableOptions.Columns, tableOptions.TableFlags, tableOptions.Name, "ct_" + tableOptions.Key)
    {
        _compendiumType = tableOptions.CompendiumType;
        _compendiumRowHeightSetting = compendiumRowHeightSetting;
        _configuration = configuration;
        ShowFilterRow = true;
        ResizeOnOpen = true;
        FreezeRows = 2;
        if (tableOptions.BuildContextMenu != null)
        {
            RightClickFunc = tableOptions.BuildContextMenu;
        }
    }

    public override List<TData> GetItems()
    {
        return _compendiumType.GetRows();
    }

    public override int RowHeight => _compendiumRowHeightSetting.CurrentValue(_configuration);
}

public sealed record CompendiumTableOptions<TData>
{
    public required string Name  { get; init; }
    public required string Key { get; init; }
    public required ICompendiumType<TData> CompendiumType { get; init; }
    public required List<IColumn<WindowState, TData, MessageBase>> Columns { get; init; }
    public Func<TData, List<MessageBase>>? BuildContextMenu { get; init; }
    public ImGuiTableFlags TableFlags { get; init; } = ImGuiTableFlags.SizingFixedFit |
                                                       ImGuiTableFlags.Resizable | ImGuiTableFlags.Hideable |
                                                       ImGuiTableFlags.Sortable | ImGuiTableFlags.RowBg |
                                                       ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.BordersOuterH |
                                                       ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.BordersOuterV |
                                                       ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersV |
                                                       ImGuiTableFlags.BordersInner | ImGuiTableFlags.BordersOuter |
                                                       ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollX |
                                                       ImGuiTableFlags.ScrollY;
}