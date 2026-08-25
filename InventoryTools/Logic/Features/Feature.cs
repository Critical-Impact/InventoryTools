using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Blocks;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Logic.Features;

public abstract class Feature : LayoutBuilder, IFeature
{
    private readonly IEnumerable<ISetting> _allSettings;

    protected Feature(IEnumerable<ISetting> settings)
    {
        _allSettings = settings;
    }

    public PageLayout Content => field ??= Build();

    public string Name => Content.Name;

    public List<ISetting> RelatedSettings => field ??= BuildSettings();

    public virtual void OnFinish()
    {
    }

    private List<ISetting> BuildSettings()
    {
        var byType = new Dictionary<Type, ISetting>();
        foreach (var setting in _allSettings) byType[setting.GetType()] = setting;

        var found = new List<ISetting>();
        Build(Content, byType, found);
        return found;
    }

    private void Build(IConfigBlock block, IReadOnlyDictionary<Type, ISetting> byType, List<ISetting> found)
    {
        if (block is SettingBlock settingNode && byType.TryGetValue(settingNode.SettingType, out var setting))
        {
            found.Add(setting);
        }

        foreach (var child in block.Children)
        {
            Build(child, byType, found);
        }

        if (block is PageLayout pageNode)
        {
            foreach (var subPage in pageNode.SubPages)
            {
                Build(subPage, byType, found);
            }
        }
    }
}
