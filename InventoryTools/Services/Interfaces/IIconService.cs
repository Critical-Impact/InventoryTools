using System;
using Dalamud.Interface.Internal;

namespace InventoryTools.Services.Interfaces;

public interface IIconService : IDisposable
{
    IDalamudTextureWrap this[int id] { get; }
    IDalamudTextureWrap LoadIcon(int id);
    IDalamudTextureWrap LoadIcon(uint id);
    IDalamudTextureWrap LoadImage(string imageName);
    bool IconExists(int id);
}