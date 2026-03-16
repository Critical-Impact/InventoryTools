using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record ItemColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, uint?> ValueSelector { get; init; }
}