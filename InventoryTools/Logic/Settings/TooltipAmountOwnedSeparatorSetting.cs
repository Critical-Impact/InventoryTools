using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipAmountOwnedSeparatorSetting : GenericSeparatorSetting
{
    public TooltipAmountOwnedSeparatorSetting(ILogger<TooltipAmountOwnedSeparatorSetting> logger, ImGuiService imGuiService) : base(
        "TooltipSeparator",
        "Add separator",
        "Add separator",
        false,
        SettingCategory.ToolTips,
        SettingSubCategory.AddItemLocations,
        "1.7.0.21",
        logger,
        imGuiService
    )
    {
    }
}