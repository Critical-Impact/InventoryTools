using System.Collections.Generic;
using CriticalCommonLib.Services;
using Dalamud.Plugin.Services;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;


namespace InventoryToolsMock;

public class MockGameInteropService : IGameInteropService
{
    private readonly ExcelSheet<ClassJob> _classJobSheet;
    private readonly IDataManager _dataManager;

    public MockGameInteropService(ExcelSheet<ClassJob> classJobSheet, IDataManager dataManager)
    {
        _classJobSheet = classJobSheet;
        _dataManager = dataManager;
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
}