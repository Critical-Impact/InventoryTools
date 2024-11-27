using CriticalCommonLib.Services.Mediator;

using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class ListsWindowHotkey : Hotkey
{
    public ListsWindowHotkey(ILogger<ListsWindowHotkey> logger, MediatorService mediatorService, InventoryToolsConfiguration configuration) : base(logger, mediatorService, configuration)
    {
    }
    public override ModifiableHotkey? ModifiableHotkey =>
        Configuration.GetHotkey(HotKeyListsWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        MediatorService.Publish(new ToggleGenericWindowMessage(typeof(FiltersWindow)));
        return true;
    }
}