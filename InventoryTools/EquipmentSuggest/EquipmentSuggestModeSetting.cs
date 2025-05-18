using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.EquipmentSuggest;

public enum EquipmentSuggestMode
{
    Class,
    Tool
}

public class EquipmentSuggestModeSetting : GenericEnumChoiceSetting<EquipmentSuggestMode>
{
    private readonly EquipmentSuggestConfig _config;

    public EquipmentSuggestModeSetting(ILogger<EquipmentSuggestModeSetting> logger, EquipmentSuggestConfig config, ImGuiService imGuiService) : base("EquipmentSuggestMode", "Mode", "What mode should the equipment recommendation screen show? Class lets you pick a class and get recommendations for it. Tool lets you pick out the tools for a set of classes.", EquipmentSuggestMode.Class, new (){
        { EquipmentSuggestMode.Class , "Class"}, { EquipmentSuggestMode.Tool , "Tool"}}, SettingCategory.EquipmentRecommendation, SettingSubCategory.General, "1.12.0.10", logger, imGuiService)
    {
        _config = config;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, EquipmentSuggestMode newValue)
    {
        _config.IsDirty = true;
        base.UpdateFilterConfiguration(configuration, newValue);
    }
}