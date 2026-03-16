using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.ItemSources;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record ItemSourceColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, List<ItemSource>> ValueSelector { get; init; }
}