using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.EquipmentSuggest;

public enum EquipmentSuggestViewMode
{
    Expanded,
    Normal,
    Compact
}

public class EquipmentSuggestViewModeSetting : GenericEnumChoiceSetting<EquipmentSuggestViewMode>
{
    public EquipmentSuggestViewModeSetting(ILogger<EquipmentSuggestViewModeSetting> logger, ImGuiService imGuiService) : base("EquipSuggestViewMode", "View Mode", "Should the equipment recommendation screen be compact or normal?", EquipmentSuggestViewMode.Normal, new(){{EquipmentSuggestViewMode.Normal, "Normal"}, {EquipmentSuggestViewMode.Expanded, "Expanded"}, {EquipmentSuggestViewMode.Compact, "Compact"}}, SettingCategory.EquipmentRecommendation, SettingSubCategory.General, "12.0.10", logger, imGuiService)
    {
    }

    public int GetIconContainerSize(InventoryToolsConfiguration configuration)
    {
        switch (CurrentValue(configuration))
        {
            case EquipmentSuggestViewMode.Expanded:
                return 64;
            case EquipmentSuggestViewMode.Normal:
                return 32;
            case EquipmentSuggestViewMode.Compact:
                return 16;
        }

        return 32;
    }

    public int GetIconSize(InventoryToolsConfiguration configuration)
    {
        switch (CurrentValue(configuration))
        {
            case EquipmentSuggestViewMode.Expanded:
                return 32;
            case EquipmentSuggestViewMode.Normal:
                return 32;
            case EquipmentSuggestViewMode.Compact:
                return 16;
        }

        return 32;
    }
}