using CriticalCommonLib.Services;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;


namespace InventoryToolsMock;

public class MockGameInteropService : IGameInteropService
{
    private readonly ExcelSheet<ClassJob> _classJobSheet;

    public MockGameInteropService(ExcelSheet<ClassJob> classJobSheet)
    {
        _classJobSheet = classJobSheet;
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
}