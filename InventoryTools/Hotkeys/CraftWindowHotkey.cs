using CriticalCommonLib.Services.Mediator;

using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class CraftWindowHotkey : Hotkey
{
    public CraftWindowHotkey(ILogger<CraftWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotkeyCraftWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(CraftsWindow)));
        return true;
    }
}