using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class DutiesWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeyDutiesWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleDutiesWindow();
        return true;
    }
}