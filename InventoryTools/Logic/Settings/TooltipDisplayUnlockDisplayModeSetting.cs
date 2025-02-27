using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public enum TooltipDisplayUnlockDisplayMode
{
    CharacterPerLine,
    GroupedByUnlockStatus
}

public class TooltipDisplayUnlockDisplayModeSetting : GenericEnumChoiceSetting<TooltipDisplayUnlockDisplayMode>
{
    public TooltipDisplayUnlockDisplayModeSetting(ILogger<TooltipDisplayUnlockDisplayModeSetting> logger, ImGuiService imGuiService) : base("TooltipDisplayUnlockDisplayMode", "Add Item Unlock Status (Display Mode)", "How should the item unlock status tooltip be displayed?", TooltipDisplayUnlockDisplayMode.CharacterPerLine, new Dictionary<TooltipDisplayUnlockDisplayMode, string>()
    {
        { TooltipDisplayUnlockDisplayMode.CharacterPerLine , "Character Per Line" },
        { TooltipDisplayUnlockDisplayMode.GroupedByUnlockStatus , "Grouped By Unlock Status" },
    }, SettingCategory.ToolTips, SettingSubCategory.ItemUnlockStatus, "1.11.1.1", logger, imGuiService)
    {
    }
}