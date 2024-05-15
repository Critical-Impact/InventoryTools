using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class MarketBoardUseActiveWorldSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; } = true;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.MarketBoardUseActiveWorld;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.MarketBoardUseActiveWorld = newValue;
    }

    public override string Key { get; set; } = "MarketBoardUseActiveWorld";
    public override string Name { get; set; } = "Price Active World?";
    public override string HelpText { get; set; } = "Should the currently active world be automatically priced?";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.MarketBoard;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Market;
    public override string Version { get; } = "1.7.0.0";

    public MarketBoardUseActiveWorldSetting(ILogger<MarketBoardUseActiveWorldSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}