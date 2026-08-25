using System.Collections.Generic;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class CollapsibleBlock : IConfigBlock
{
    private readonly bool _defaultOpen;

    public CollapsibleBlock(string title, bool defaultOpen, IReadOnlyList<IConfigBlock> children)
    {
        Title = title;
        _defaultOpen = defaultOpen;
        Children = children;
    }

    public string Title { get; }
    public IReadOnlyList<IConfigBlock> Children { get; }

    public void Draw(ConfigDrawContext context)
    {
        var flags = _defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        if (ImGui.CollapsingHeader(Title, flags))
        {
            ImGui.Indent();
            try
            {
                foreach (var child in Children) child.Draw(context);
            }
            finally
            {
                ImGui.Unindent();
            }

            ImGui.Spacing();
        }
    }
}