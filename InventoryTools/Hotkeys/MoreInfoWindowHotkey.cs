using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class MoreInfoWindowHotkey : Hotkey
{
    private readonly ItemSheet _itemSheet;

    public MoreInfoWindowHotkey(ILogger<MoreInfoWindowHotkey> logger, MediatorService mediatorService, ItemSheet itemSheet, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
        _itemSheet = itemSheet;
    }
    public override ModifiableHotkey? ModifiableHotkey => Configuration.MoreInformationHotKey;

    public override bool OnHotKey()
    {
        var id = Service.GameGui.HoveredItem;
        if (id >= 2000000 || id == 0) return false;
        id %= 500000;
        var item = _itemSheet.GetRowOrDefault((uint) id);
        if (item == null) return false;
        MediatorService.Publish(new ToggleUintWindowMessage(typeof(ItemWindow), item.RowId));
        return true;
    }
}