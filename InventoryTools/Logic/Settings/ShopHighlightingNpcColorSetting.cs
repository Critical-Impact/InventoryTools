using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class ShopHighlightingNpcColorSetting : GenericEnumChoiceSetting<ObjectHighlightColor>
{
    public ShopHighlightingNpcColorSetting(ILogger<ShopHighlightingNpcColorSetting> logger, ImGuiService imGuiService) : base("ShopHighlightingNpcColor", "Shop Highlighting - NPC Highlight Color", "The color used to highlight NPCs in the world when shop items are highlighted.", ObjectHighlightColor.Green, new Dictionary<ObjectHighlightColor, string>
    {
        { ObjectHighlightColor.Red,     "Red"     },
        { ObjectHighlightColor.Green,   "Green"   },
        { ObjectHighlightColor.Blue,    "Blue"    },
        { ObjectHighlightColor.Yellow,  "Yellow"  },
        { ObjectHighlightColor.Orange,  "Orange"  },
        { ObjectHighlightColor.Magenta, "Magenta" },
    }, "15.0.4", logger, imGuiService)
    {
    }
}
