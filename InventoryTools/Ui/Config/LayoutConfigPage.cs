using System;
using System.Collections.Generic;
using System.Linq;
using DalaMock.Host.Mediator;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Config.Blocks;
using InventoryTools.Ui.Config.Layouts;

namespace InventoryTools.Ui.Config;

public class LayoutConfigPage : IConfigPage
{
    private readonly InventoryToolsConfiguration _configuration;
    private readonly ConfigNavigationState _navigationState;
    private readonly ImGuiService _imGuiService;
    private readonly IReadOnlyDictionary<Type, ISetting> _settings;
    private readonly PageLayout _root;

    public delegate LayoutConfigPage Factory(IConfigLayout layout);

    public LayoutConfigPage(IConfigLayout layout,
        IEnumerable<ISetting> settings,
        ConfigNavigationState navigationState,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration)
    {
        _configuration = configuration;
        _navigationState = navigationState;
        _imGuiService = imGuiService;

        var byType = new Dictionary<Type, ISetting>();
        foreach (var setting in settings) byType[setting.GetType()] = setting;

        _settings = byType;
        _root = layout.Build();
        ChildPages = BuildChildPages(_root);
    }

    private LayoutConfigPage(PageLayout layout,
        IReadOnlyDictionary<Type, ISetting> settings,
        ConfigNavigationState navigationState,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration)
    {
        _configuration = configuration;
        _navigationState = navigationState;
        _imGuiService = imGuiService;
        _settings = settings;
        _root = layout;
        ChildPages = BuildChildPages(_root);
    }

    private IEnumerable<IConfigPage>? BuildChildPages(PageLayout layout)
    {
        return layout.SubPages.Count == 0
            ? null
            : layout.SubPages
                .Select(c => (IConfigPage)new LayoutConfigPage(c, _settings, _navigationState, _imGuiService, _configuration))
                .ToList();
    }

    public void Initialize()
    {
    }

    public string Key => _root.Key;
    public string Name => _root.Name;
    public bool IsMenuItem => false;
    public IEnumerable<IConfigPage>? ChildPages { get; set; }
    public bool DrawBorder => true;

    public IEnumerable<ConfigSearchEntry> GetSearchEntries()
    {
        var entries = new List<ConfigSearchEntry>();
        foreach (var child in _root.Children)
        {
            Build(child, null, entries);
        }

        return entries;
    }

    private void Build(IConfigBlock block, string? sectionTitle, ICollection<ConfigSearchEntry> entries)
    {
        var section = block switch
        {
            SectionBlock sectionNode => sectionNode.Title,
            CollapsibleBlock collapsibleNode => collapsibleNode.Title,
            _ => sectionTitle
        };

        if (block is SettingBlock settingNode && _settings.TryGetValue(settingNode.SettingType, out var setting))
            entries.Add(new ConfigSearchEntry(
                settingNode.SettingType,
                settingNode.NameOverride ?? setting.Name,
                setting.HelpText,
                _root.Key,
                _root.Name,
                section));

        foreach (var child in block.Children)
        {
            Build(child, section, entries);
        }
    }

    public List<MessageBase>? Draw()
    {
        _root.Draw(new ConfigDrawContext(_configuration, _settings, _navigationState, _imGuiService));
        return null;
    }
}