using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class ScrollableBlock : IConfigBlock
{
    private readonly string _id;
    private readonly float _height;

    public ScrollableBlock(string id, float height, IReadOnlyList<IConfigBlock> children)
    {
        _id = id;
        _height = height;
        Children = children;
    }

    public IReadOnlyList<IConfigBlock> Children { get; }

    public void Draw(ConfigDrawContext context)
    {
        using var child = ImRaii.Child(_id,
            new Vector2(0, _height) * ImGui.GetIO().FontGlobalScale,
            true);
        if (!child.Success) return;
        foreach (var node in Children)
        {
            node.Draw(context);
        }
    }
}