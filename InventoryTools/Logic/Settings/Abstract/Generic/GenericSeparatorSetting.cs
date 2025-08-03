using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericSeparatorSetting : BooleanSetting
{
    public GenericSeparatorSetting(
        string key,
        string name,
        string helpText,
        bool? defaultValue,
        SettingCategory settingCategory,
        SettingSubCategory settingSubCategory,
        string version, 
        ILogger logger, 
        ImGuiService imGuiService
        ) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue ?? false;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }
    
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipSeparator;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipSeparator = newValue;
    }

    public override string Key { get; set; }
    public override string Name { get; set; }
    public override string HelpText { get; set; }
    public override SettingCategory SettingCategory { get; set; }
    public override SettingSubCategory SettingSubCategory { get; }
    public override string Version { get; }
}
