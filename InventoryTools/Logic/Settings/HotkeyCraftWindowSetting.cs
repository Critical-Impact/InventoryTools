using Dalamud.Game.ClientState.Keys;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Logic.Settings
{
    public class HotkeyCraftWindowSetting : HotKeySetting
    {
        public override ModifiableHotkey DefaultValue { get; set; } = new(VirtualKey.NO_KEY);
        public static string AsKey => "HotkeyCraftWindow";
        public override string Key { get; set; } = AsKey;
        public override string Name { get; set; } = "Toggle Craft Window";

        public override string HelpText { get; set; } =
            "The hotkey to toggle the craft window.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Hotkeys;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public HotkeyCraftWindowSetting(ILogger<HotkeyCraftWindowSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}