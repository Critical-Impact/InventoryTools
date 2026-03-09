using System;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumBooleanColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, bool?> ValueSelector { get; init; }
}