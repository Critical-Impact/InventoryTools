using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using InstanceContent = Lumina.Excel.Sheets.InstanceContent;
using PublicContent = Lumina.Excel.Sheets.PublicContent;

namespace InventoryTools.Services;

public interface IUIStateService
{
    public bool IsInstanceContentCompleted(InstanceContent row);
    public bool IsPublicContentCompleted(PublicContent row);
}

public class UIStateService : IUIStateService
{
    private readonly IPlayerState _playerState;

    public UIStateService(IPlayerState playerState)
    {
        _playerState = playerState;
    }
    
    public bool IsInstanceContentCompleted(InstanceContent row)
    {
        if (!_playerState.IsLoaded)
        {
            return false;
        }

        return UIState.IsInstanceContentCompleted(row.RowId);
    }

    public bool IsPublicContentCompleted(PublicContent row)
    {
        if (!_playerState.IsLoaded)
        {
            return false;
        }

        return UIState.IsPublicContentCompleted(row.RowId);
    }
}