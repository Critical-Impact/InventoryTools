using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config.ConfigLayouts;

public class HotkeysLayout : ConfigLayout
{
    public override PageLayout Build()
    {
        return Page("hotkeys", "Hotkeys",
            Section("Toggle a window",
                Paragraph("Work anywhere, whether or not you are hovering something."),
                Setting<HotKeyListsWindowSetting>("Lists"),
                Setting<HotkeyCraftWindowSetting>("Crafts"),
                Setting<HotkeyConfigWindowSetting>("Configuration"),
                Setting<HotkeyMobWindowSetting>("Mobs"),
                Setting<HotkeyDutiesWindowSetting>("Duties"),
                Setting<HotkeyAirshipWindowSetting>("Airships"),
                Setting<HotkeySubmarinesWindowSetting>("Submarines"),
                Setting<HotkeyRetainerTasksWindowSetting>("Retainer ventures")),
            Section("While hovering an item",
                Paragraph("These act on whatever item is under the cursor, so they only do anything while an item is hovered."),
                Setting<HotkeyMoreInfoSetting>("More information"),
                Setting<HotkeyOpenItemLogSetting>("Whichever log applies"),
                Setting<HotkeyOpenCraftingLogSetting>("Crafting log"),
                Setting<HotkeyOpenGatheringLogSetting>("Gathering log"),
                Setting<HotkeyOpenFishingLogSetting>("Fishing log"))
        );
    }
}
