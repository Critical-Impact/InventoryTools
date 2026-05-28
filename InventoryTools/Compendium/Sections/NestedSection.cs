using System.Linq;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Services;
using OtterGui.Raii;
using ImRaii = Dalamud.Interface.Utility.Raii.ImRaii;

namespace InventoryTools.Compendium.Sections;

public class NestedSection : ViewSection
{
    private readonly NestedSectionOptions _options;

    public delegate NestedSection Factory(NestedSectionOptions options);

    public NestedSection(NestedSectionOptions options, ImGuiService imGuiService) : base(options, imGuiService)
    {
        _options = options;
    }

    public override string SectionName => _options.SectionName;

    public override bool IsEmpty(SectionState sectionState) =>
        _options.Sections.All(s => !s.ShouldDraw(sectionState));

    public override void DrawSection(SectionState sectionState)
    {
        using var indent = ImRaii.PushIndent(1);
        for (var index = 0; index < _options.Sections.Count; index++)
        {
            using var id = ImRaii.PushId(index);
            var section = _options.Sections[index];
            if (!section.IsEmpty(sectionState))
            {
                section.Draw(sectionState);
            }
        }
    }
}
