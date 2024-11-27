using CriticalCommonLib.Services.Mediator;

using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class AirshipsWindowHotkey : Hotkey
{
    public AirshipsWindowHotkey(ILogger<AirshipsWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeyAirshipWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(AirshipsWindow)));
        return true;
    }
}