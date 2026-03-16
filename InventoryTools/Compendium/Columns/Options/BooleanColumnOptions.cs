using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record BooleanColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, bool?> ValueSelector { get; init; }
}