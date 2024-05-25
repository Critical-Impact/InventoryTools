using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class MoreInfoWindowHotkey : Hotkey
{
    private readonly ExcelCache _excelCache;

    public MoreInfoWindowHotkey(ILogger<MoreInfoWindowHotkey> logger, MediatorService mediatorService, ExcelCache excelCache, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
        _excelCache = excelCache;
    }
    public override ModifiableHotkey? ModifiableHotkey => Configuration.MoreInformationHotKey;

    public override bool OnHotKey()
    {
        var id = Service.GameGui.HoveredItem;
        if (id >= 2000000 || id == 0) return false;
        id %= 500000;
        var item = _excelCache.GetItemExSheet().GetRow((uint) id);
        if (item == null) return false;
        MediatorService.Publish(new ToggleUintWindowMessage(typeof(ItemWindow), item.RowId));
        return true;
    }
}