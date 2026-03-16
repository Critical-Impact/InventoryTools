using Dalamud.Bindings.ImGui;

namespace InventoryTools.Compendium.Columns.Options;

public abstract record ColumnOptions
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