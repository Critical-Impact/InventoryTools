using System.Runtime.CompilerServices;
using Dalamud;
using InventoryTools.Services;
using Lumina;
using Lumina.Data.Files;
using Lumina.Extensions;

namespace InventoryToolsMock;

public class MockDataService : IDataService
{
    private GameData _gameData;
    public MockDataService(GameData gameData)
    {
        _gameData = gameData;
    }
    public TexFile? GetHqIcon(uint iconId)
    {
        return _gameData.GetHqIcon(iconId);
    }

    public TexFile? GetIcon(uint iconId)
    {
        return _gameData.GetIcon(iconId);
    }

    public TexFile? GetIcon(string? type, uint iconId)
    {
        if (type == null)
            type = string.Empty;
        if (type.Length > 0 && !type.EndsWith("/"))
            type += "/";
        TexFile file = _gameData.GetFile<TexFile>(string.Format("ui/icon/{0:D3}000/{1}{2:D6}.tex", (object) (iconId / 1000U), (object) type, (object) iconId));
        return type == string.Empty || file != null ? file : _gameData.GetFile<TexFile>(string.Format("ui/icon/{0:D3}000/{1}{2:D6}.tex", (object) (iconId / 1000U), (object) string.Empty, (object) iconId));
    }

    public TexFile? GetIcon(bool isHq, uint iconId)
    {
        return this.GetIcon(isHq ? "hq/" : string.Empty, iconId);
    }

    public TexFile? GetIcon(ClientLanguage iconLanguage, uint iconId)
    {
        string type;
        switch (iconLanguage)
        {
            case ClientLanguage.Japanese:
                type = "ja/";
                break;
            case ClientLanguage.English:
                type = "en/";
                break;
            case ClientLanguage.German:
                type = "de/";
                break;
            case ClientLanguage.French:
                type = "fr/";
                break;
            default:
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
                interpolatedStringHandler.AppendLiteral("Unknown Language: ");
                interpolatedStringHandler.AppendFormatted<ClientLanguage>(iconLanguage);
                throw new ArgumentOutOfRangeException(nameof (iconLanguage), interpolatedStringHandler.ToStringAndClear());
        }
        return this.GetIcon(type, iconId);
    }

    public TexFile? GetUldIcon(string iconName)
    {
        return _gameData.GetFile<TexFile>(string.Format("ui/uld/{0}.tex", iconName));
    }
}