using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ShopHighlightingNpcSetting : GenericBooleanSetting
{
    public ShopHighlightingNpcSetting(ILogger<ShopHighlightingNpcSetting> logger, ImGuiService imGuiService) : base("ShopHighlightingNpc", "Shop Highlighting - Highlight NPCs", "When highlighting items in a shop, should related NPCs be highlighted in the world?", true, "15.0.4", logger, imGuiService)
    {
    }
}
