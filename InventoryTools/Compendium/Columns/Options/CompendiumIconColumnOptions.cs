using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumIconColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, int> ValueSelector { get; init; }
}