using CriticalCommonLib.Services;
using InventoryTools.Services;
using Lumina.Excel.GeneratedSheets;

namespace InventoryToolsMock;

public class MockGameInteropService : IGameInteropService
{
    private readonly ExcelCache _excelCache;

    public MockGameInteropService(ExcelCache excelCache)
    {
        _excelCache = excelCache;
    }
    
    public unsafe Dictionary<ClassJob, short>? GetClassJobLevels()
    {
        return new Dictionary<ClassJob, short>()
        {
            {_excelCache.GetClassJobSheet().GetRow(1)!,30},
            {_excelCache.GetClassJobSheet().GetRow(2)!,60},
            {_excelCache.GetClassJobSheet().GetRow(3)!,50},
            {_excelCache.GetClassJobSheet().GetRow(4)!,80},
            {_excelCache.GetClassJobSheet().GetRow(5)!,90},
            {_excelCache.GetClassJobSheet().GetRow(6)!,10},
        };
    }
}