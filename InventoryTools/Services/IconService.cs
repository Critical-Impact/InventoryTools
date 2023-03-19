using System.IO;
using InventoryTools.Services.Interfaces;

namespace InventoryTools.Services;

using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Data.Files;

public class IconService : IIconService
{
    private readonly DalamudPluginInterface        _pi;
    private readonly DataManager                   _gameData;
    private readonly Dictionary<uint, TextureWrap> _icons;
    private readonly Dictionary<string, TextureWrap> _images;

    public IconService(DalamudPluginInterface pi, DataManager gameData, int size = 0)
    {
        _pi       = pi;
        _gameData = gameData;
        _icons    = new Dictionary<uint, TextureWrap>(size);
        _images    = new Dictionary<string, TextureWrap>(size);
    }

    public TextureWrap this[int id]
        => LoadIcon(id);

    private TexFile? LoadIconHq(uint id)
    {
        var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}_hr1.tex";
        return _gameData.GetFile<TexFile>(path);
    }

    public TextureWrap LoadIcon(int id)
        => LoadIcon((uint)id);

    public TextureWrap LoadIcon(uint id)
    {
        if (_icons.TryGetValue(id, out var ret))
            return ret;

        var icon     = LoadIconHq(id) ?? _gameData.GetIcon(id)!;
        var iconData = icon.GetRgbaImageData();

        ret        = _pi.UiBuilder.LoadImageRaw(iconData, icon.Header.Width, icon.Header.Height, 4);
        _icons[id] = ret;
        return ret;
    }

    public TextureWrap LoadImage(string imageName)
    {
        if (_images.TryGetValue(imageName, out var ret))
            return ret;
        
        var assemblyLocation = PluginService.PluginInterfaceService!.AssemblyLocation.DirectoryName!;
        var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");
        var textureWrap = PluginService.PluginInterfaceService!.LoadImage(imagePath);
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
        var icon     = LoadIconHq((uint)id) ?? _gameData.GetIcon((uint)id);
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
