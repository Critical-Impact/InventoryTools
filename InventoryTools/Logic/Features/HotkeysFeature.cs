using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class HotkeysFeature : Feature
{
    public HotkeysFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(HotkeyMoreInfoSetting),
            typeof(HotkeyConfigWindowSetting),
            typeof(HotkeyAirshipWindowSetting),
            typeof(HotkeyCraftWindowSetting),
            typeof(HotkeyDutiesWindowSetting),
            typeof(HotkeyMobWindowSetting),
            typeof(HotkeySubmarinesWindowSetting),
            typeof(HotkeyRetainerTasksWindowSetting),
            typeof(HotKeyListsWindowSetting),
            typeof(HotkeyOpenGatheringLogSetting),
            typeof(HotkeyOpenCraftingLogSetting),
            typeof(HotkeyOpenFishingLogSetting),
            typeof(HotkeyOpenItemLogSetting),
        },
        settings)
    {
    }

    public string Version { get; } = "1.0.0.0";
    public override string Name { get; } = "Hotkeys";

    public override string Description { get; } =
        "Set hotkeys for opening the various Allagan Tools windows. A hotkey to open the 'More Information' window for items is also available.";
}