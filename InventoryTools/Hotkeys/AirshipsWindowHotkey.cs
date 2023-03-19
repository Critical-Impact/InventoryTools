using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class AirshipsWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeyAirshipWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleAirshipsWindow();
        return true;
    }
}