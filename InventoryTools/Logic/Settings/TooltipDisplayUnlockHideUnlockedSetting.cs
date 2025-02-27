using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayUnlockHideUnlockedSetting : GenericBooleanSetting
{
    public TooltipDisplayUnlockHideUnlockedSetting(ILogger<TooltipDisplayUnlockHideUnlockedSetting> logger, ImGuiService imGuiService) : base("TooltipDisplayUnlockHideUnlocked", "Add Item Unlock Status (Hide Unlocked Characters)", "Should characters that already have this unlocked be hidden? If in grouped mode, this will hide the acquired group.", false, SettingCategory.ToolTips, SettingSubCategory.ItemUnlockStatus, "1.11.1.1", logger, imGuiService)
    {
    }
}