using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class TooltipsFeature : Feature
{
    public TooltipsFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(TooltipDisplayAmountOwnedSetting),
            typeof(TooltipLocationDisplayModeSetting),
            typeof(TooltipDisplayRetrieveAmountSetting),
            typeof(TooltipMinimumMarketPriceSetting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Tooltips";
    public override string Description { get; } =
        "Allagan Tools can add extra information to the tooltips for items. It can add the amount you own of an item(including retainers), the amount you should retrieve(when using craft lists) and also pricing from the market.";
}