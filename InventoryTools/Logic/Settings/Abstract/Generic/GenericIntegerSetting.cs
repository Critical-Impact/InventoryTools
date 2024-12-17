using InventoryTools.Logic.Filters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericIntegerSetting : IntegerSetting
{
    public GenericIntegerSetting(string key, string name, string helpText, int defaultValue, SettingCategory settingCategory, SettingSubCategory settingSubCategory, string version, ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }

    public sealed override int DefaultValue { get; set; }
    public override int CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, DefaultValue) ?? DefaultValue;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
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