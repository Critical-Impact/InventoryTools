using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class RetainerTasksWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeyRetainerTasksWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleRetainerTasksWindow();
        return true;
    }
}