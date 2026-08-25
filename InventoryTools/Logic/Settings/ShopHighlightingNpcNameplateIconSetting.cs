using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ShopHighlightingNpcNameplateIconSetting : GenericBooleanSetting
{
    public ShopHighlightingNpcNameplateIconSetting(ILogger<ShopHighlightingNpcNameplateIconSetting> logger, ImGuiService imGuiService)
        : base(
            "ShopHighlightingNpcNameplateIcon",
            "Shop Highlighting - Nameplate Icon",
            "When highlighting shop items, show a small vendor icon on the nameplate of related NPCs in the world.",
            true,
            "15.0.4",
            logger,
            imGuiService)
    {
    }
}
