using CriticalCommonLib;
using DalaMock;
using DalaMock.Dalamud;
using DalaMock.Mock;
using Dalamud.Interface.Internal;
using Dalamud.Utility;
using ImGuiScene;
using InventoryTools;
using InventoryTools.Services.Interfaces;
using Lumina.Data.Files;
using Lumina.Extensions;
using Veldrid;

namespace InventoryToolsMock;

public class MockIconService : IIconService
{

    private Lumina.GameData _gameData;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiController _imGuiController;

    public MockIconService(Lumina.GameData gameData, MockProgram mockProgram)
    {
        _gameData = gameData;
        _graphicsDevice = mockProgram.GraphicsDevice;
        _imGuiController = mockProgram.Controller;
        _icons = new Dictionary<uint, IDalamudTextureWrap>();
        _images    = new Dictionary<string, IDalamudTextureWrap>();
    }
    
    public void Dispose()
    {
    }
    
    private readonly Dictionary<uint, IDalamudTextureWrap> _icons;
    private readonly Dictionary<string, IDalamudTextureWrap> _images;

    public IDalamudTextureWrap this[int id] => LoadIcon(id);

    public IDalamudTextureWrap LoadIcon(int id)
    {
        if (_icons.TryGetValue((uint)id, out var ret))
            return ret;

        var icon     = LoadIconHq((uint)id) ?? _gameData.GetIcon((uint)id)!;
        var iconData = icon.GetRgbaImageData();
        var texture = _graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)icon.TextureBuffer.Width, (uint)icon.TextureBuffer.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        var CPUframeBufferTextureId = _imGuiController.GetOrCreateImGuiBinding(_graphicsDevice.ResourceFactory, texture);
        _graphicsDevice.UpdateTexture(texture, iconData, 0,0,0, (uint)icon.TextureBuffer.Width, (uint)icon.TextureBuffer.Height, 1, 0,0);
        var veldridTextureWrap =
            new MockTextureMap(CPUframeBufferTextureId, icon.TextureBuffer.Width, icon.TextureBuffer.Height);
        _icons[(uint)id] = veldridTextureWrap;
        return veldridTextureWrap;
    }

    private HashSet<int> _availableIcons = new HashSet<int>();
    private HashSet<int> _unAvailableIcons = new HashSet<int>();

    public IDalamudTextureWrap LoadImage(string imageName)
    {
        if (_images.TryGetValue(imageName, out var ret))
            return ret;
        
        var assemblyLocation = Service.Interface!.AssemblyLocation.DirectoryName!;
        var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");
        var loadedImage = Service.TextureProvider.GetTextureFromFile(new FileInfo(imagePath));
        var dalamudTextureWrap = new MockTextureMap(loadedImage.ImGuiHandle, loadedImage.Width, loadedImage.Height);
        _images[imageName] = dalamudTextureWrap;
        return  dalamudTextureWrap;
    }

    public bool IconExists(int id)
    {
        if (_availableIcons.Contains(id))
        {
            return true;
        }
        if (_unAvailableIcons.Contains(id))
        {
            return false;
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
    
    private TexFile? LoadIconHq(uint id)
    {
        var path = $"ui/icon/{id / 1000 * 1000:000000}/{id:000000}_hr1.tex";
        return _gameData.GetFile<TexFile>(path);
    }

    public IDalamudTextureWrap LoadIcon(uint id)
    {
        return LoadIcon((int)id);
    }
}