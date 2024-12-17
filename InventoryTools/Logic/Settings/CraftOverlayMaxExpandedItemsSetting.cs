using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class CraftOverlayMaxExpandedItemsSetting : GenericIntegerSetting
{
    public CraftOverlayMaxExpandedItemsSetting(ILogger<CraftOverlayMaxExpandedItemsSetting> logger,
        ImGuiService imGuiService) : base("CraftOverlayMaxItems",
        "Max items when expanded",
        "When the craft overlay is expanded, how many items should be shown?",
        5,
        SettingCategory.CraftOverlay,
        SettingSubCategory.General,
        "1.11.0.8",
        logger,
        imGuiService)
    {
    }
}