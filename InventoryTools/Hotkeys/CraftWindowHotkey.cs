using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class CraftWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey =>
        ConfigurationManager.Config.GetHotkey(HotkeyCraftWindowSetting.AsKey);
    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleCraftsWindow();
        return true;
    }
}