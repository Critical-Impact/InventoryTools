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

public class MobWindowHotkey : Hotkey
{
    public MobWindowHotkey(ILogger<MobWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeyMobWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(BNpcsWindow)));
        return true;
    }
}