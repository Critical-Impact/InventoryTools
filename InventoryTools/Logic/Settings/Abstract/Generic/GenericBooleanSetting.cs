using InventoryTools.Logic.Filters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericBooleanSetting : BooleanSetting
{
    public GenericBooleanSetting(string key, string name, string helpText, bool defaultValue, SettingCategory settingCategory, SettingSubCategory settingSubCategory, string version, ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }

    public sealed override bool DefaultValue { get; set; }
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, DefaultValue) ?? DefaultValue;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.Set(Key, newValue);
    }

    public sealed override string Key { get; set; }
    public sealed override string Name { get; set; }
    public sealed override string HelpText { get; set; }
    public sealed override SettingCategory SettingCategory { get; set; }
    public override SettingSubCategory SettingSubCategory { get; }
    public override string Version { get; }
}