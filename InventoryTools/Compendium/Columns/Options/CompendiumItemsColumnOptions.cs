using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumItemsColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, List<ItemRow>> ValueSelector { get; init; }
}