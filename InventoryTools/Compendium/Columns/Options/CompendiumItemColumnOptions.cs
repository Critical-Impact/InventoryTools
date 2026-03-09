using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumItemColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, uint?> ValueSelector { get; init; }
}