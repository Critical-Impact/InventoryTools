using System;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumOpenViewTableColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, (string?, uint?)> ValueSelector { get; init; }
    public required Func<TData, uint> RowIdSelector { get; init; }
    public required ICompendiumType CompendiumType { get; init; }
}