using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record ItemsColumnOptions<TData> : ColumnOptions
{
    public required Func<TData, List<ItemRow>> ValueSelector { get; init; }
}