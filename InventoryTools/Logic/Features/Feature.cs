using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public abstract class Feature : IFeature
{
    public Feature(IEnumerable<Type> applicableSettings, IEnumerable<ISetting> settings)
    {
        var settingsHashSet = applicableSettings.ToHashSet();
        RelatedSettings = new();
        RelatedSettings = settings.Where(c => settingsHashSet.Contains(c.GetType())).ToList();
    }
    public List<ISetting> RelatedSettings { get;  }
    public abstract string Name { get; }
    public abstract string Description { get; }

    public virtual void OnFinish()
    {
        
    }
}