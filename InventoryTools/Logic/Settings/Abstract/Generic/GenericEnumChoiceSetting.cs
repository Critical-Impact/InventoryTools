using System;
using System.Collections.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericEnumChoiceSetting<TEnum> : ChoiceSetting<TEnum>
    where TEnum : Enum, IComparable
{
    protected GenericEnumChoiceSetting(string key, string name, string helpText, TEnum defaultValue, Dictionary<TEnum, string> choices, SettingCategory settingCategory, SettingSubCategory settingSubCategory, string version, ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        Choices = choices;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }

    public override TEnum CurrentValue(InventoryToolsConfiguration configuration)
    {
        return (TEnum)(configuration.Get(Key, DefaultValue) ?? DefaultValue);
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, TEnum newValue)
    {
        configuration.Set(Key, newValue);
    }

    public sealed override string Key { get; set; }
    public sealed override string Name { get; set; }
    public sealed override string HelpText { get; set; }
    public sealed override TEnum DefaultValue { get; set; }
    public sealed override SettingCategory SettingCategory { get; set; }
    public override SettingSubCategory SettingSubCategory { get; }
    public override Dictionary<TEnum, string> Choices { get; }
    public override string Version { get; }
}