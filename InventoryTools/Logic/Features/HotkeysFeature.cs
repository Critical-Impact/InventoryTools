using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class HotkeysFeature : Feature
{
    public HotkeysFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/hotkeys", "Hotkeys",
            Paragraph("These hotkeys are optional. If you leave a field empty, that action has no hotkey."),
            Section("Show or hide a window",
                Setting<HotKeyListsWindowSetting>("Lists"),
                Setting<HotkeyCraftWindowSetting>("Crafts"),
                Setting<HotkeyConfigWindowSetting>("Configuration"),
                Setting<HotkeyMobWindowSetting>("Mobs"),
                Setting<HotkeyDutiesWindowSetting>("Duties"),
                Setting<HotkeyAirshipWindowSetting>("Airships"),
                Setting<HotkeySubmarinesWindowSetting>("Submarines"),
                Setting<HotkeyRetainerTasksWindowSetting>("Retainer ventures")),
            Section("While the cursor is over an item",
                Paragraph("These hotkeys apply to the item under the cursor. If there is no item under the cursor, they do nothing."),
                Setting<HotkeyMoreInfoSetting>("More information"),
                Setting<HotkeyOpenItemLogSetting>("The log that applies to the item"),
                Setting<HotkeyOpenCraftingLogSetting>("Crafting log"),
                Setting<HotkeyOpenGatheringLogSetting>("Gathering log"),
                Setting<HotkeyOpenFishingLogSetting>("Fishing log"))
        );
    }
}
