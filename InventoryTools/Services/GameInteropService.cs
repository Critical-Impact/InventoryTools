using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;
using Lumina.Excel.Sheets;


namespace InventoryTools.Services;

public interface IGameInteropService
{
    unsafe Dictionary<ClassJob, short>? GetClassJobLevels();
}

public class GameInteropService : IGameInteropService
{
    private readonly IClientState _clientState;
    private readonly ExcelSheet<ClassJob> _classJobSheet;

    public GameInteropService(IClientState clientState, ExcelSheet<ClassJob> classJobSheet)
    {
        _clientState = clientState;
        _classJobSheet = classJobSheet;
    }

    public unsafe Dictionary<ClassJob, short>? GetClassJobLevels()
    {
        if (!_clientState.IsLoggedIn)
        {
            return null;
        }

        var levels = new Dictionary<ClassJob, short>();

        var byExpArray = _classJobSheet.Where(c => c.RowId != 0).DistinctBy(c => c.ExpArrayIndex).ToDictionary(c => (short)c.ExpArrayIndex, c => c);
        var span = UIState.Instance()->PlayerState.ClassJobLevels;
        for (short index = 0; index < span.Length; index++)
        {
            var level = span[index];
            if (byExpArray.TryGetValue(index, out var value))
            {
                levels.Add(value, level);
            }
        }

        return levels;
    }
}