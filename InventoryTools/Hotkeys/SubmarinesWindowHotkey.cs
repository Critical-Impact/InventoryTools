using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class SubmarinesWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeySubmarinesWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleSubmarinesWindow();
        return true;
    }
}