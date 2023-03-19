using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class MobWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeyMobWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleMobWindow();
        return true;
    }
}