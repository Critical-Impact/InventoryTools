using System;
using System.Collections.Generic;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record MapLinksColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, List<MapLinkEntry>> ValueSelector { get; init; }
}
