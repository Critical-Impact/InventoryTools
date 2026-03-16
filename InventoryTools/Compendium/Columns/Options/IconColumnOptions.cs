using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record IconColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, int> ValueSelector { get; init; }
}