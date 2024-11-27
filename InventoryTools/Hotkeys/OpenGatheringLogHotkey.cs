using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class OpenGatheringLogHotkey : Hotkey
{
    private readonly ItemSheet _itemSheet;
    private readonly IGameInterface _gameInterface;

    public OpenGatheringLogHotkey(ILogger<OpenGatheringLogHotkey> logger, MediatorService mediatorService, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameInterface gameInterface) : base(logger, mediatorService, configuration)
    {
        _itemSheet = itemSheet;
        _gameInterface = gameInterface;
    }
    public override ModifiableHotkey? ModifiableHotkey => Configuration.OpenGatheringLogHotKey;

    public override bool OnHotKey()
    {
        var id = Service.GameGui.HoveredItem;
        if (id >= 2000000 || id == 0) return false;
        id %= 500000;
        var item = _itemSheet.GetRowOrDefault((uint) id);
        if (item == null || !item.CanOpenGatheringLog) return false;
        _gameInterface.OpenGatheringLog(item.RowId);
        return true;
    }
}