using Dalamud.Game.ClientState.Keys;
using InventoryTools.Logic.Settings.Abstract;
using OtterGui.Classes;

namespace InventoryTools.Logic.Settings
{
    public class HotkeyMoreInfoSetting : HotKeySetting
    {
        public override ModifiableHotkey DefaultValue { get; set; } = new(VirtualKey.M, ModifierHotkey.Control); 
        
        public override ModifiableHotkey CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.MoreInformationHotKey ?? new ModifiableHotkey();
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, ModifiableHotkey newValue)
        {
            configuration.MoreInformationHotKey = newValue;
        }

        public override string Key { get; set; } = "MoreInformationHotKey";
        public override string Name { get; set; } = "More Information Hotkey";

        public override string HelpText { get; set; } =
            "The hotkey to open the more information window for an item when hovering it.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Hotkeys;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
    }
}