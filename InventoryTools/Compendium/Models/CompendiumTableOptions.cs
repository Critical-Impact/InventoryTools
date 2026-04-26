using System;
using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public sealed record CompendiumTableOptions<TData>
{
    public required string Name  { get; init; }
    public required string Key { get; init; }
    public required ICompendiumType<TData> CompendiumType { get; init; }
    public required Func<(ICompendiumGrouping<TData>, object?)?, List<IColumn<WindowState, TData, MessageBase>>> Columns { get; init; }
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