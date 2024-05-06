using System.Collections.Generic;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public class BasicFeature : Feature
{
    public BasicFeature(IEnumerable<ISetting> settings) : base(new[]
        {
            typeof(AutoSaveSetting),
            typeof(AllowCrossCharacterSetting),
            typeof(HistoryEnabledSetting),
            typeof(AddTitleMenuButtonSetting)
        },
        settings)
    {
    }
    
    public override string Name { get; } = "Basic";
    public override string Description { get; } = "Configure the basic settings of Allagan Tools";
}