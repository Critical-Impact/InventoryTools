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

public class SubmarinesWindowHotkey : Hotkey
{
    public SubmarinesWindowHotkey(ILogger<SubmarinesWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeySubmarinesWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow)));
        return true;
    }
}