using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Features;

public interface IFeature
{
    List<ISetting> RelatedSettings { get; }
    string Name { get; }
    string Description { get; }

    void OnFinish();
}