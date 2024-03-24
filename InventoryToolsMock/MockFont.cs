using DalaMock.Shared.Interfaces;
using ImGuiNET;

namespace InventoryToolsMock;

public class MockFont : IFont
{
    public ImFontPtr DefaultFont { get; } = new ImFontPtr();
    public ImFontPtr IconFont { get; } = new ImFontPtr();
    public ImFontPtr MonoFont { get; } = new ImFontPtr();
}