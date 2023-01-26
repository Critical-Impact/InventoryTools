using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings;

public partial class TooltipDisplayRetrieveAmountSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; }
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipDisplayRetrieveAmount;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipDisplayRetrieveAmount = newValue;
    }

    public override string Key { get; set; } = "DisplayRetrievalAmount";
    public override string Name { get; set; } = "Display Retrieval Amount?";

    public override string HelpText { get; set; } =
        "Should the amount required to be retrieved be shown in the tooltip?";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Tooltips;
}