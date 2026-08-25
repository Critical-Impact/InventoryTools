using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public interface IFeature
{
    PageLayout Content { get; }

    List<ISetting> RelatedSettings { get; }

    string Name { get; }

    void OnFinish();
}
