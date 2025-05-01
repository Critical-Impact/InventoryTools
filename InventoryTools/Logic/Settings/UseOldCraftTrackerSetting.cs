using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class UseOldCraftTrackerSetting : GenericBooleanSetting
{
    public UseOldCraftTrackerSetting(ILogger<UseOldCraftTrackerSetting> logger, ImGuiService imGuiService) : base("UseOldCraftTracker", "Use Old Craft Tracker?", "Use the old craft tracker, this only tracks crafts and uses a different mechanism to the new acquisition tracker.", false, SettingCategory.CraftTracker, SettingSubCategory.General, "1.12.0.4", logger, imGuiService)
    {
    }
}