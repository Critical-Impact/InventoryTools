using System.Collections.Generic;
using System.Linq;
using Autofac;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class FiltersFeature : Feature
{
    public FiltersFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(SampleFilter100GillOrLess),
            typeof(SampleFilterDuplicateItems),
            typeof(SampleFilterMaterialCleanup),
        },
        settings)
    {
    }

    public override string Name { get; } = "Sample Item Lists";
    public override string Description { get; } = "Select which sample item lists you'd like to install by default. These are good examples of the types of lists that are possible within Allagan Tools.";

    public override void OnFinish()
    {
        foreach (var setting in RelatedSettings.Select(c => c as ISampleFilter))
        {
            if (setting != null && setting.ShouldAdd)
            {
                setting.AddFilter();
            }
        }
    }

}

//Need to add in hide category or make category optional/null
//Need to add in put in armoire/glamour sample
//Maybe other samples?
