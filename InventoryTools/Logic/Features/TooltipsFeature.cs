using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class TooltipsFeature : Feature
{
    public TooltipsFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(TooltipDisplayAmountOwnedSetting),
            typeof(TooltipMinimumMarketPriceSetting),
            typeof(TooltipDisplayUnlockSetting),
            typeof(TooltipSourceInformationEnabledSetting),
            typeof(TooltipUseInformationEnabledSetting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Tooltips";
    public override string Description { get; } =
        "Allagan Tools can add extra information to the tooltips for items. Select which you would like to show in the tooltip. For further configuration including the ability to change each tooltips colour and settings specific to each tooltip please open the configuration window.";
}