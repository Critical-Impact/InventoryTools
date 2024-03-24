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

public class RetainerTasksWindowHotkey : Hotkey
{
    public RetainerTasksWindowHotkey(ILogger<RetainerTasksWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeyRetainerTasksWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
        return true;
    }
}