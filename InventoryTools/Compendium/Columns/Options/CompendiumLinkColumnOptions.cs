using System;
using Lumina.Excel;

namespace InventoryTools.Compendium.Columns.Options;

public sealed record CompendiumLinkColumnOptions<TData> : ColumnOptions
{
    /// <summary>
    /// The text to display. Filtering, sorting and CSV export use this value.
    /// </summary>
    public required Func<TData, string?> ValueSelector { get; init; }

    /// <summary>
    /// The row the text links to. When no compendium type can open the row, the text is drawn without a link.
    /// </summary>
    public required Func<TData, RowRef?> RowRefSelector { get; init; }
}
