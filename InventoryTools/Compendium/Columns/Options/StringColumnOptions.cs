using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record StringColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, string?> ValueSelector { get; init; }
}