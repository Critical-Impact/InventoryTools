using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class LayoutFeature : Feature
{
    public LayoutFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(CraftWindowLayoutSetting),
            typeof(FiltersWindowLayoutSetting),
        },
        settings)
    {
    }

    public override string Name { get; } = "Layout";
    public override string Description { get; } =
        "How should the main items window and craft windows be laid out? Should we display your lists as tabs or in a side bar?";
}