using ImGuiScene;

namespace InventoryToolsMock;

public class VeldridTextureMap : TextureWrap
{

    public VeldridTextureMap(nint handle, int width, int height)
    {
        ImGuiHandle = handle;
        Width = width;
        Height = height;
    }
    public void Dispose()
    {
    }

    public nint ImGuiHandle { get; }
    public int Width { get; }
    public int Height { get; }
}