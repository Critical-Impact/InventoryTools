using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class LayoutFeature : Feature
{
    public LayoutFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/layout", "Layout",
            Paragraph("The items window and the craft window can show your lists as tabs along the top, or in a side bar. A side bar is better when you have many lists. Tabs are better when you have only a few."),
            Setting<FiltersWindowLayoutSetting>("Items window"),
            Setting<CraftWindowLayoutSetting>("Craft window")
        );
    }
}
