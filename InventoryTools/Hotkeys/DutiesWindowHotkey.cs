using CriticalCommonLib.Services.Mediator;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Lumina;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InventoryTools.Hotkeys;

public class DutiesWindowHotkey : Hotkey
{
    public DutiesWindowHotkey(ILogger<DutiesWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeyDutiesWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(DutiesWindow)));
        return true;
    }
}