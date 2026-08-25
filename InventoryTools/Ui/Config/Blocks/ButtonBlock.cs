using System;
using System.Collections.Generic;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class ButtonBlock : IConfigBlock
{
    private readonly string _label;
    private readonly Func<MessageBase> _message;

    public ButtonBlock(string label, Func<MessageBase> message)
    {
        _label = label;
        _message = message;
    }

    public IReadOnlyList<IConfigBlock> Children => [];

    public void Draw(ConfigDrawContext context)
    {
        if (ImGui.Button(_label))
        {
            context.Messages.Enqueue(_message());
        }

        ImGui.Spacing();
    }
}