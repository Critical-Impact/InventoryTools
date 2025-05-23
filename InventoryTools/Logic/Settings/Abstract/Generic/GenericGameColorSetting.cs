using InventoryTools.Logic.Filters;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings.Abstract.Generic;

public abstract class GenericGameColorSetting : GameColorSetting
{
    public GenericGameColorSetting(string key, string name, string helpText, uint? defaultValue, SettingCategory settingCategory, SettingSubCategory settingSubCategory, string version, ILogger logger, ImGuiService imGuiService, ExcelSheet<UIColor> uiColorSheet) : base(logger, imGuiService, uiColorSheet)
    {
        Key = key;
        Name = name;
        HelpText = helpText;
        DefaultValue = defaultValue;
        SettingCategory = settingCategory;
        SettingSubCategory = settingSubCategory;
        Version = version;
    }

    public sealed override uint? DefaultValue { get; set; }
    public override uint? CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.Get(Key, DefaultValue) ?? DefaultValue;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, uint? newValue)
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