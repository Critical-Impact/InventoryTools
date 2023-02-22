using System;
using ImGuiScene;

namespace InventoryTools.Services;

public interface IIconService : IDisposable
{
    TextureWrap this[int id] { get; }
    TextureWrap LoadIcon(int id);
    TextureWrap LoadIcon(uint id);
    bool IconExists(int id);
}