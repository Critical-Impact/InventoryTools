using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public class FiltersFeature : Feature
{
    public FiltersFeature(IEnumerable<ISetting> settings) : base(settings)
    {
    }

    public override PageLayout Build()
    {
        return Page("feature/sample-lists", "Sample Lists",
            Paragraph("These lists show you what the plugin can do. After you add them, they behave as normal lists. You can change or delete them at any time."),
            Setting<SampleFilter100GillOrLess>("Items worth 100 gil or less"),
            Setting<SampleFilterDuplicateItems>("Duplicate items in your inventory"),
            Setting<SampleFilterMaterialCleanup>("Crafting materials to move to storage")
        );
    }

    public override void OnFinish()
    {
        foreach (var setting in RelatedSettings.Select(c => c as ISampleFilter))
        {
            if (setting is { ShouldAdd: true })
            {
                setting.AddFilter();
            }
        }
    }
}