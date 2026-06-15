using System.Collections.Generic;
using CriticalCommonLib.Services;
using Dalamud.Plugin.Services;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;


namespace InventoryToolsMock;

public class MockGameInteropService : IGameInteropService
{
    private readonly ExcelSheet<ClassJob> _classJobSheet;
    private readonly IDataManager _dataManager;
    private readonly ILogger<MockGameInteropService> _logger;

    public MockGameInteropService(ExcelSheet<ClassJob> classJobSheet, IDataManager dataManager, ILogger<MockGameInteropService> logger)
    {
        _classJobSheet = classJobSheet;
        _dataManager = dataManager;
        _logger = logger;
    }

    public unsafe Dictionary<ClassJob, short>? GetClassJobLevels()
    {
        return new Dictionary<ClassJob, short>()
        {
            {_classJobSheet.GetRow(1),30},
            {_classJobSheet.GetRow(2),60},
            {_classJobSheet.GetRow(3),50},
            {_classJobSheet.GetRow(4),80},
            {_classJobSheet.GetRow(5),90},
            {_classJobSheet.GetRow(6),10},
        };
    }

    public byte? GetChocoboStainId()
    {
        return 7; //Always return pink
    }

    public unsafe RowRef<Stain> GetChocoboStain()
    {
        return new RowRef<Stain>(_dataManager.Excel, GetChocoboStainId() ?? 0);
    }

    public void PlayChatSoundEffect(uint soundEffectId)
    {
        _logger.LogInformation("Would play chat sound effect {SoundEffectId}", soundEffectId);
    }
}