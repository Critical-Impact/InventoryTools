using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets.Rows;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Compendium.Models;

public abstract record CompendiumColumnOptions
{
    public required string Name { get; init; }
    public required string Key { get; init; }
    public required string HelpText { get; init; }
    public required string Version { get; init; }
    public int Width { get; init; } = 100;
    public bool HideFilter { get; init; }
    public string? RenderName { get; init; } = null;
    public string EmptyText { get; init; } = "";
    public ImGuiTableColumnFlags ColumnFlags { get; init; } = ImGuiTableColumnFlags.None;
}

public sealed record CompendiumStringColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, string?> ValueSelector { get; init; }
}
public sealed record CompendiumIntegerColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, string?> ValueSelector { get; init; }
}
public sealed record CompendiumItemSourceColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, List<ItemSource>> ValueSelector { get; init; }
}
public sealed record CompendiumItemsColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, List<ItemRow>> ValueSelector { get; init; }
}
public sealed record CompendiumIconColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, int?> ValueSelector { get; init; }
}
public sealed record CompendiumBooleanColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, bool?> ValueSelector { get; init; }
}
public sealed record CompendiumItemColumnOptions<TData> : CompendiumColumnOptions
{
    public required Func<TData, uint?> ValueSelector { get; init; }
}
