using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using OtterGui.Classes;

namespace InventoryTools.Hotkeys;

public class ConfigurationWindowHotkey : Hotkey
{
    public override ModifiableHotkey? ModifiableHotkey => ConfigurationManager.Config.GetHotkey(HotkeyConfigWindowSetting.AsKey);

    public override bool OnHotKey()
    {
        PluginService.WindowService.ToggleConfigurationWindow();
        return true;
    }
}