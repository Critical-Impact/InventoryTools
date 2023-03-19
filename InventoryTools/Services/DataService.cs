using Dalamud;
using Dalamud.Data;
using InventoryTools.Extensions;
using InventoryTools.Services.Interfaces;
using Lumina.Data.Files;

namespace InventoryTools.Services;

public class DataService : IDataService
{
    private DataManager _dataManager;
    public DataService(DataManager dataManager)
    {
        _dataManager = dataManager;
    }
    public TexFile? GetHqIcon(uint iconId)
    {
        return _dataManager.GetHqIcon(iconId);
    }

    public TexFile? GetIcon(uint iconId)
    {
        return _dataManager.GetIcon(iconId);
    }

    public TexFile? GetIcon(string? type, uint iconId)
    {
        return _dataManager.GetIcon(type, iconId);
    }

    public TexFile? GetIcon(bool isHq, uint iconId)
    {
        return _dataManager.GetIcon(isHq, iconId);
    }

    public TexFile? GetIcon(ClientLanguage iconLanguage, uint iconId)
    {
        return _dataManager.GetIcon(iconLanguage, iconId);
    }

    public TexFile? GetUldIcon(string iconName)
    {
        return _dataManager.GetUldIcon(iconName);
    }
}