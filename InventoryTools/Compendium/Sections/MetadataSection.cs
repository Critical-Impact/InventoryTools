using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Services;

namespace InventoryTools.Compendium.Sections;

public sealed class MetadataSection : ViewSection
{
    private readonly List<MetadataSectionOptions.Row> _rows;

    public delegate MetadataSection Factory(MetadataSectionOptions options);

    public MetadataSection(MetadataSectionOptions options, ImGuiService imGuiService)
        : base(imGuiService)
    {
        SectionName = options.SectionName;
        _rows = options.Rows;
    }

    public override string SectionName { get; }

    public override bool ShouldDraw(SectionState sectionState)
    {
        foreach (var row in _rows)
        {
            if (row.ShouldDraw == null || row.ShouldDraw())
                return true;
        }

        return false;
    }

    public override void DrawSection(SectionState sectionState)
    {
        using var table = ImRaii.Table("MetadataTable", 2,
            ImGuiTableFlags.SizingStretchSame |
            ImGuiTableFlags.PadOuterX |
            ImGuiTableFlags.NoHostExtendX);

        ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

        if (!table)
            return;

        foreach (var row in _rows)
        {
            if (row.ShouldDraw != null && !row.ShouldDraw())
                continue;

            var value = row.Value();
            if (string.IsNullOrEmpty(value))
                continue;

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(row.Label);

            ImGui.TableNextColumn();
            ImGui.TextWrapped(value);
        }
    }
}