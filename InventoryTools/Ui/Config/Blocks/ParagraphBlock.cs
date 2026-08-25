using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class ParagraphBlock : IConfigBlock
{
    private readonly string _text;

    public ParagraphBlock(string text)
    {
        _text = text;
    }

    public IReadOnlyList<IConfigBlock> Children => [];

    public void Draw(ConfigDrawContext context)
    {
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey))
        {
            ImGui.TextWrapped(_text);
        }

        ImGui.Spacing();
    }
}