using System;
using System.Collections.Generic;
using AllaganLib.Shared.Extensions;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class LinkBlock : IConfigBlock
{
    private readonly string _label;
    private readonly string _url;

    public LinkBlock(string label, string url)
    {
        _label = label;
        _url = url;
    }

    public IReadOnlyList<IConfigBlock> Children => [];

    public void Draw(ConfigDrawContext context)
    {
        if (ImGui.Button(_label))
        {
            _url.OpenBrowser();
        }

        ImGui.Spacing();
    }
}