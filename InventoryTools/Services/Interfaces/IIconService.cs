using System;
using ImGuiScene;

namespace InventoryTools.Services.Interfaces;

public interface IIconService : IDisposable
{
    TextureWrap this[int id] { get; }
    TextureWrap LoadIcon(int id);
    TextureWrap LoadIcon(uint id);
    TextureWrap LoadImage(string imageName);
    bool IconExists(int id);
}