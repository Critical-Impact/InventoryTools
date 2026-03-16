using System;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record OpenViewTableColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, (string?, uint?)> ValueSelector { get; init; }
    public required Func<TData, uint> RowIdSelector { get; init; }
    public required ICompendiumType CompendiumType { get; init; }
}