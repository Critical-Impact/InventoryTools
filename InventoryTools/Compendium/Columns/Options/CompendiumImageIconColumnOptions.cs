using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumImageIconColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, string> ValueSelector { get; init; }
}