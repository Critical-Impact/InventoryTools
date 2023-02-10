using Dalamud;
using Lumina.Data.Files;

namespace InventoryTools.Services;

public interface IDataService
{
    public TexFile? GetHqIcon(uint iconId);
    
    public TexFile? GetIcon(uint iconId);
    public TexFile? GetIcon(string? type, uint iconId);
    public TexFile? GetIcon(bool isHq, uint iconId);
    public TexFile? GetIcon(ClientLanguage iconLanguage, uint iconId);

    public TexFile? GetUldIcon(string iconName);
}