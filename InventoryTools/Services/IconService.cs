using System.IO;
using CriticalCommonLib;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

using System.Collections.Generic;

public class IconService : IIconService
{
    private readonly ITextureProvider _textureProvider;
    private readonly Dictionary<uint, IDalamudTextureWrap> _icons;
    private readonly Dictionary<string, IDalamudTextureWrap> _images;

    public IconService(ITextureProvider textureProvider, int size = 0)
    {
        _textureProvider = textureProvider;
        _icons    = new Dictionary<uint, IDalamudTextureWrap>(size);
        _images    = new Dictionary<string, IDalamudTextureWrap>(size);
    }

    public IDalamudTextureWrap this[int id]
        => LoadIcon(id);

    public IDalamudTextureWrap LoadIcon(int id)
        => LoadIcon((uint)id);

    public IDalamudTextureWrap LoadIcon(uint id)
    {
        if (_icons.TryGetValue(id, out var ret))
            return ret;

        var icon     = _textureProvider.GetIcon(id, ITextureProvider.IconFlags.ItemHighQuality) ?? _textureProvider.GetIcon(id)!;

        _icons[id] = icon;
        return icon;
    }

    public IDalamudTextureWrap LoadImage(string imageName)
    {
        if (_images.TryGetValue(imageName, out var ret))
            return ret;
        
        var assemblyLocation = Service.Interface!.AssemblyLocation.DirectoryName!;
        var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");
        var textureWrap = _textureProvider.GetTextureFromFile(new FileInfo(imagePath))!;
        _images[imageName] = textureWrap;
        return  textureWrap;
    }

    private HashSet<int> _availableIcons = new HashSet<int>();
    private HashSet<int> _unAvailableIcons = new HashSet<int>();

    public bool IconExists(int id)
    {
        if (_availableIcons.Contains(id))
        {
            return true;
        }
        if (_unAvailableIcons.Contains(id))
        {
            return true;
        }
        var icon     = _textureProvider.GetIcon((uint)id, ITextureProvider.IconFlags.ItemHighQuality) ?? _textureProvider.GetIcon((uint)id);
        if (icon == null)
        {
            _unAvailableIcons.Add(id);
        }
        if (icon != null)
        {
            _availableIcons.Add(id);
        }
        return icon != null;
    }

    public void Dispose()
    {
        foreach (var icon in _icons.Values)
            icon.Dispose();
        foreach (var image in _images.Values)
            image.Dispose();
        _icons.Clear();
        _images.Clear();
    }

    ~IconService()
        => Dispose();
}
