using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class GatedBlock : IConfigBlock
{
    private readonly Type _gateType;

    public GatedBlock(Type gateType, IReadOnlyList<IConfigBlock> children)
    {
        _gateType = gateType;
        Children = children;
    }

    public IReadOnlyList<IConfigBlock> Children { get; }

    public void Draw(ConfigDrawContext context)
    {
        var enabled = context.Find(_gateType) is Setting<bool> gate
                      && gate.CurrentValue(context.Configuration);

        ImGui.Indent();
        try
        {
            using (ImRaii.Disabled(!enabled))
            {
                foreach (var child in Children) child.Draw(context);
            }
        }
        finally
        {
            ImGui.Unindent();
        }
    }
}