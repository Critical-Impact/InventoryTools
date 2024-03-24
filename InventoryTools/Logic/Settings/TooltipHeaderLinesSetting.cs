using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipHeaderLinesSetting : IntegerSetting
{
    public override int DefaultValue { get; set; } = 0;
    public override int CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipHeaderLines;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
    {
        configuration.TooltipHeaderLines = newValue;
    }

    public override string Key { get; set; } = "TooltipDisplayHeader";
    public override string Name { get; set; } = "Header New Lines";

    public override string HelpText { get; set; } =
        "How many new lines should be added above any tooltip modifications made by this plugin?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Visuals;
    public override string Version => "1.6.2.5";

    public TooltipHeaderLinesSetting(ILogger<TooltipHeaderLinesSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}