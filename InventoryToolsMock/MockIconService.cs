using Dalamud.Utility;
using ImGuiScene;
using InventoryTools.Services;
using Lumina.Data.Files;
using Lumina.Extensions;
using Veldrid;

namespace InventoryToolsMock;

public class MockIconService : IIconService
{

    private Lumina.GameData _gameData;
    public MockIconService(Lumina.GameData gameData)
    {
        _gameData = gameData;
        _icons = new Dictionary<uint, TextureWrap>();
    }
    
    public void Dispose()
    {
    }
    
    private readonly Dictionary<uint, TextureWrap> _icons;

    public TextureWrap this[int id] => LoadIcon(id);

    public TextureWrap LoadIcon(int id)
    {
        if (_icons.TryGetValue((uint)id, out var ret))
            return ret;

        var icon     = LoadIconHq((uint)id) ?? _gameData.GetIcon((uint)id)!;
        var iconData = icon.GetRgbaImageData();
        var texture = Program._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)icon.TextureBuffer.Width, (uint)icon.TextureBuffer.Height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
        var CPUframeBufferTextureId = Program._controller.GetOrCreateImGuiBinding(Program._gd.ResourceFactory, texture);
        Program._gd.UpdateTexture(texture, iconData, 0,0,0, (uint)icon.TextureBuffer.Width, (uint)icon.TextureBuffer.Height, 1, 0,0);
        var veldridTextureWrap =
            new VeldridTextureMap(CPUframeBufferTextureId, icon.TextureBuffer.Width, icon.TextureBuffer.Height);
        _icons[(uint)id] = veldridTextureWrap;
        return veldridTextureWrap;
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

    public TextureWrap LoadIcon(uint id)
    {
        return LoadIcon((int)id);
    }
}