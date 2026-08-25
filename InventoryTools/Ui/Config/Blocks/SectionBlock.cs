using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class SectionBlock : IConfigBlock
{
    public SectionBlock(string title, IReadOnlyList<IConfigBlock> children)
    {
        Title = title;
        Children = children;
    }

    public string Title { get; }
    public IReadOnlyList<IConfigBlock> Children { get; }

    public void Draw(ConfigDrawContext context)
    {
        ImGui.Spacing();
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow))
        {
            ImGui.TextUnformatted(Title);
        }

        ImGui.Separator();
        ImGui.Spacing();

        foreach (var child in Children)
        {
            child.Draw(context);
        }

        ImGui.Spacing();
    }
}