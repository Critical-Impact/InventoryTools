using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericMultipleEnumChoiceSetting<TEnum> : MultipleEnumChoiceSetting<TEnum>
    where TEnum : struct, Enum, IComparable
{
    protected GenericMultipleEnumChoiceSetting(string key, string name, string helpText, List<TEnum> defaultValue, Dictionary<TEnum, string> choices, string version, ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        Choices = choices;
        Version = version;
    }

    public override List<TEnum> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get<TEnum>(Key, DefaultValue) ?? DefaultValue;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<TEnum> newValue)
    {
        configuration.Set(Key, newValue);
    }

    public sealed override string Key { get; set; }
    public sealed override string Name { get; set; }
    public sealed override string HelpText { get; set; }
    public sealed override List<TEnum> DefaultValue { get; set; }
    public sealed override Dictionary<TEnum, string> Choices { get; }
    public override string Version { get; }
}
