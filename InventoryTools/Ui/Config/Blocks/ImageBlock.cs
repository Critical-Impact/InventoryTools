using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui.Config.Blocks;

public sealed class ImageBlock : IConfigBlock
{
    private readonly string _imageName;
    private readonly float _width;
    private readonly float _height;

    public ImageBlock(string imageName, float width, float height)
    {
        _imageName = imageName;
        _width = width;
        _height = height;
    }

    public IReadOnlyList<IConfigBlock> Children => [];

    public void Draw(ConfigDrawContext context)
    {
        var texture = context.ImGuiService.GetImageTexture(_imageName);
        ImGui.Image(texture.Handle, new Vector2(_width, _height) * ImGui.GetIO().FontGlobalScale);
        ImGui.Spacing();
    }
}